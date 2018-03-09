using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Properties; 
//using System.Web.Http;
using System.Web.Http.Results;
using ScreenViewer.API;
using ScreenViewer.Models;
using System.IO;
using System.Text;
using ScreenViewer.Data;
using System.Text.RegularExpressions;
using ScreenViewer.Models.Elements;
using ScreenViewer.API.Elements;
using ScreenViewer.SessionControl;

namespace ScreenViewer.Controllers
{
    public class SectionController : Controller
    {
        //
        // GET: /Section/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Display(decimal id)
        {
            ScreenViewer.API.SectionController SC = new API.SectionController();
            var actionResult = SC.GetScriptSection(id);
            var response = actionResult as OkNegotiatedContentResult<ScriptSection>;
            var theSection = response.Content;

            return Display(theSection.SectionName);
        }

        [ActionName("DisplayByUniqueName")]
        public ActionResult Display(string id)
        {

            ScreenViewer.API.SectionController SC = new API.SectionController();
            var actionResult = SC.GetScriptSection(id);
            var response = actionResult as OkNegotiatedContentResult<ScriptSection>;

            if (response != null)
            {
                ScriptSection theSection = response.Content;
                theSection.ScriptSectionLayouts = theSection.ScriptSectionLayouts.OrderBy(o => o.Sequence).ToList();

                string qincluded = "";
                bool critItems = false;
                CriticalElements Cel = GetCriticalElements(theSection,out critItems);


                List<SectionViewLayout> SectionLayout = new List<SectionViewLayout>();

                foreach (ScriptSectionLayout sectionitem in theSection.ScriptSectionLayouts)
                {
                    if (sectionitem.ShowWhen != null && sectionitem.ShowWhen != 0)
                    {
                            ClauseEvaluator CE = new ClauseEvaluator(HttpContext.Session);
                            bool continueSection = CE.EvaluateClause(sectionitem.ShowWhen.ToString(), Convert.ToInt32(sectionitem.ShowIf));
                            if (continueSection)
                            {
                                SectionViewLayout SVL = new SectionViewLayout();
                                SVL.SectionLayout = sectionitem;
                                
                                 
                                if (sectionitem.ElementType == "Question")
                                {
                                    qincluded = qincluded == "" ? sectionitem.ElementID.ToString() : qincluded + "," +  sectionitem.ElementID.ToString(); 
                                
                                    if (Cel.CriticalItems)
                                    {
                                       if (Cel.CriticalQuestions.Contains(sectionitem.ElementID.ToString()))
                                       {
                                           SVL.CriticalElement = true;
                                       }
                                    }
                                }
                                else if (sectionitem.ElementType == "Action")
                                {
                                    ActionController AC = new ActionController();
                                    var actResult = AC.GetScriptAction(Convert.ToDecimal(sectionitem.ElementID));
                                    var actResponse = actResult as OkNegotiatedContentResult<ScriptAction>;
                                    ScriptAction SA = (ScriptAction)actResponse.Content;

                                    if (!SA.ActionName.StartsWith("CLICKABLE"))
                                        FireActions(string.Format("{0}::{1}", SA.ScriptActionID.ToString(), SA.ActionName));
                                }
                                else if(sectionitem.ElementType == "OrderItem" && critItems)
                                {
                                    SVL.CriticalElement = true;
                                }
                                else
                                {
                                    SVL.CriticalElement = false;
                                }
                                SectionLayout.Add(SVL);
                            }
                     }
                    else
                    {
                        SectionViewLayout SVL2 = new SectionViewLayout();
                        SVL2.SectionLayout = sectionitem;
                                
                        if (sectionitem.ElementType == "Question")
                        {
                            qincluded = qincluded == "" ? sectionitem.ElementID.ToString() : qincluded + "," + sectionitem.ElementID.ToString();

                            if (Cel.CriticalItems)
                            {
                                if (Cel.CriticalQuestions.Contains(sectionitem.ElementID.ToString()))
                                {
                                    SVL2.CriticalElement = true;
                                }
                            }                      
                        }
                        else if (sectionitem.ElementType == "OrderItem" && critItems)
                        {
                            SVL2.CriticalElement = true;
                        }
                        else
                        {
                            SVL2.CriticalElement = false;
                        }
                        SectionLayout.Add(SVL2);

                    }

                }

                QuestionController QuestController = new QuestionController();
                QuestionLayout questionLayout = null;
                foreach (SectionViewLayout layout in SectionLayout)
                {

                    switch (layout.SectionLayout.ElementType)
                    {
                        case "Question":

                            questionLayout = QuestController.RenderQuestionParts((decimal)layout.SectionLayout.ElementID, layout.CriticalElement, ControllerContext);

                            questionLayout.QuestionHTML = questionLayout.QuestionHTML.Replace("\r", "");
                            questionLayout.QuestionHTML = questionLayout.QuestionHTML.Replace("\n", "");
                            questionLayout.QuestionHTML = questionLayout.QuestionHTML.Replace(@"""", @"\""");
                            questionLayout.QuestionHTML = questionLayout.QuestionHTML.Replace("</script>", string.Format("\" + {0} + \"", "unescape('%3C/script%3E')"));
                            questionLayout.QuestionLabelText = questionLayout.QuestionLabelText.Replace("\r", "");
                            questionLayout.QuestionLabelText = questionLayout.QuestionLabelText.Replace("\n", "");
                            questionLayout.QuestionLabelText = questionLayout.QuestionLabelText.Replace(@"""", @"\""");
                            layout.QuestionLayout = questionLayout;
                            break;

                        case "Questionare":

                            QuestionareController QC = new QuestionareController();
                            questionLayout = new QuestionLayout();
                            //questionLayout.QuestionLabelText = "Questionare";
                            questionLayout.QuestionHTML = QC.RenderQuestionParts((decimal)layout.SectionLayout.ElementID, Cel, ControllerContext);
                            questionLayout.QuestionHTML = questionLayout.QuestionHTML.Replace("\r", "");
                            questionLayout.QuestionHTML = questionLayout.QuestionHTML.Replace("\n", "");
                            questionLayout.QuestionHTML = questionLayout.QuestionHTML.Replace(@"""", @"\""");
                            layout.QuestionLayout = questionLayout;
                            break;
                        case "Action":
                            ActionController AC = new ActionController();
                            var actResult = AC.GetScriptAction(Convert.ToDecimal(layout.SectionLayout.ElementID));
                            var actResponse = actResult as OkNegotiatedContentResult<ScriptAction>;
                            ScriptAction SA = (ScriptAction)actResponse.Content;
                            //if (SA.ActionName.StartsWith("CLICKABLE"))
                            //{
                                ActionLayout AL = new ActionLayout();
                                AL.actionId = SA.ScriptActionID.ToString();
                                AL.actionDisplay = SA.ActionDesc;
                                AL.actionName = SA.ActionName;
                            //AL.IsActionClicked = SessionManager.IsActionClicked(Session, AL.actionId);
                            //AL.AgentId = SessionManager.GetScriptParameterByKey("AgentID", Session);
                            //AL.LeadId = SessionManager.GetProgramParameterByKey("LeadID", Session);

                            //FireActions(string.Format("{0}::{1}", SA.ScriptActionID.ToString(), SA.ActionName));
                            layout.ActionLayout = AL;
                            //}
                            break;
                        case "Html":
                            TextHTMLController TextHC = new TextHTMLController();
                            layout.TextHTMLLayout = TextHC.RenderHtml(Convert.ToInt32(layout.SectionLayout.ElementID), ControllerContext);
                            layout.TextHTMLLayout = HttpUtility.HtmlDecode(layout.TextHTMLLayout);
                            layout.TextHTMLLayout = layout.TextHTMLLayout.Replace("\r", "");
                            layout.TextHTMLLayout = layout.TextHTMLLayout.Replace("\n", "");
                            layout.TextHTMLLayout = layout.TextHTMLLayout.Replace(@"""", @"\""");
                            layout.TextHTMLLayout = layout.TextHTMLLayout.Replace(@"\\", @"\");
                            layout.TextHTMLLayout = layout.TextHTMLLayout.Replace("</script>", string.Format("\" + {0} + \"", "unescape('%3C/script%3E')"));
                            layout.TextHTMLLayout = layout.TextHTMLLayout.Replace("'s", "&apos;s");
                            break;
                        case "TextHTML":
                            TextHTMLController THC = new TextHTMLController();
                            layout.TextHTMLLayout = THC.Render(Convert.ToInt32(layout.SectionLayout.ElementID), ControllerContext);
                            layout.TextHTMLLayout = HttpUtility.HtmlDecode(layout.TextHTMLLayout);
                            layout.TextHTMLLayout = layout.TextHTMLLayout.Replace("\r", "");
                            layout.TextHTMLLayout = layout.TextHTMLLayout.Replace("\n", "");
                            layout.TextHTMLLayout = layout.TextHTMLLayout.Replace(@"""", @"\""");
                            layout.TextHTMLLayout = layout.TextHTMLLayout.Replace(@"\\", @"\");
                            layout.TextHTMLLayout = layout.TextHTMLLayout.Replace("</script>", string.Format("\" + {0} + \"", "unescape('%3C/script%3E')"));
                            layout.TextHTMLLayout = layout.TextHTMLLayout.Replace("'s", "&apos;s");
                            break;
                        case "OrderCart":
                            OrderCartController OCC = new OrderCartController();
                            layout.OrderCartLayout = OCC.Render(ControllerContext);
                            layout.OrderCartLayout = HttpUtility.HtmlDecode(layout.OrderCartLayout);
                            layout.OrderCartLayout = layout.OrderCartLayout.Replace("\r", "");
                            layout.OrderCartLayout = layout.OrderCartLayout.Replace("\n", "");
                            layout.OrderCartLayout = layout.OrderCartLayout.Replace(@"""", @"\""");
                            layout.OrderCartLayout = layout.OrderCartLayout.Replace(@"\\", @"\");
                            layout.OrderCartLayout = layout.OrderCartLayout.Replace("</script>", string.Format("\" + {0} + \"", "unescape('%3C/script%3E')"));
                            layout.OrderCartLayout = layout.OrderCartLayout.Replace("'s", "&apos;s");
                            break;
                        case "DataView":
                            DataViewController DVC = new DataViewController();
                            layout.DataViewLayout = DVC.Render(Convert.ToInt32(layout.SectionLayout.ElementID), ControllerContext);
                            layout.DataViewLayout = HttpUtility.HtmlDecode(layout.DataViewLayout);
                            layout.DataViewLayout = layout.DataViewLayout.Replace("\r", "");
                            layout.DataViewLayout = layout.DataViewLayout.Replace("\n", "");
                            layout.DataViewLayout = layout.DataViewLayout.Replace(@"""", @"\""");
                            layout.DataViewLayout = layout.DataViewLayout.Replace(@"\\", @"\");
                            layout.DataViewLayout = layout.DataViewLayout.Replace("</script>", string.Format("\" + {0} + \"", "unescape('%3C/script%3E')"));
                            layout.DataViewLayout = layout.DataViewLayout.Replace("'s", "&apos;s");
                            break;
                        case "ItemSelector":
                            OrderItemController OIC = new OrderItemController();
                            layout.OrderItemLayout = OIC.Render(Convert.ToInt32(layout.SectionLayout.ElementID), layout.CriticalElement, ControllerContext);
                            layout.OrderItemLayout = layout.OrderItemLayout.Replace("\r", "");
                            layout.OrderItemLayout = layout.OrderItemLayout.Replace("\n", "");
                            layout.OrderItemLayout = layout.OrderItemLayout.Replace(@"""", @"\""");
                            layout.OrderItemLayout = layout.OrderItemLayout.Replace("</script>", string.Format("\" + {0} + \"", "unescape('%3C/script%3E')"));
                            break;
                        case "Link":
                            LinkController LC = new LinkController();
                            layout.LinkLayout = LC.Render(Convert.ToInt32(layout.SectionLayout.ElementID), ControllerContext);
                            layout.LinkLayout = layout.LinkLayout.Replace("\r", "");
                            layout.LinkLayout = layout.LinkLayout.Replace("\n", "");
                            layout.LinkLayout = layout.LinkLayout.Replace(@"""", @"\""");
                            layout.LinkLayout = layout.LinkLayout.Replace("</script>", string.Format("\" + {0} + \"", "unescape('%3C/script%3E')"));
                            break;
                        case "LinkGroup":
                            LinkGroupController LGC = new LinkGroupController();
                            layout.LinkGroupLayout = LGC.Render(Convert.ToInt32(layout.SectionLayout.ElementID), ControllerContext);
                            layout.LinkGroupLayout = layout.LinkGroupLayout.Replace("\r", "");
                            layout.LinkGroupLayout = layout.LinkGroupLayout.Replace("\n", "");
                            layout.LinkGroupLayout = layout.LinkGroupLayout.Replace(@"""", @"\""");
                            layout.LinkGroupLayout = layout.LinkGroupLayout.Replace("</script>", string.Format("\" + {0} + \"", "unescape('%3C/script%3E')"));
                            break;
                        case "Image":
                            ImageController image = new ImageController();
                            layout.ImageLayout = image.Render(Convert.ToInt32(layout.SectionLayout.ElementID), ControllerContext);
                            layout.ImageLayout = layout.ImageLayout.Replace("\r", "");
                            layout.ImageLayout = layout.ImageLayout.Replace("\n", "");
                            layout.ImageLayout = layout.ImageLayout.Replace(@"""", @"\""");
                            layout.ImageLayout = layout.ImageLayout.Replace(@"\\", @"\");
                            break;
                        case "Task":
                            TaskController task = new TaskController();
                            layout.TaskLayout = task.Render(Convert.ToInt32(layout.SectionLayout.ElementID), ControllerContext);
                            layout.TaskLayout = layout.TaskLayout.Replace("\r", "");
                            layout.TaskLayout = layout.TaskLayout.Replace("\n", "");
                            layout.TaskLayout = layout.TaskLayout.Replace(@"""", @"\""");
                            layout.TaskLayout = layout.TaskLayout.Replace("</script>", string.Format("\" + {0} + \"", "unescape('%3C/script%3E')"));
                            break;
                        default:
                            break;
                    }
                }
                List<string> PopUps = new List<string>();
                //PopUps = new List<string>();
                //PopUps.Add("http://www.microsoft.com");
                //PopUps.Add("http://www.verizon.com");
                ViewBag.ExternalPages = PopUps;

                ViewBag.QuestionList = qincluded;

                string viewTemplate = "_SectionLayout?View";

                if (Session["layout"] != null)
                    viewTemplate = Session["layout"].ToString(); //viewTemplate.Replace("?", Session["layout"].ToString());
                else
                    viewTemplate = "SectionPreview";


                List<SectionViewLayout> orderedLayout = SectionLayout.OrderBy(o => o.SectionLayout.Sequence).ToList();


                //if (this.Request.Path.Contains("/Section/"))
                //    return View(viewTemplate, SectionLayout);
                //else
                return PartialView(viewTemplate, orderedLayout);
            }
            else
            {
                return null;
            }
        }

        public ActionResult Preview(decimal id)
        {
            ScreenViewer.API.SectionController SC = new API.SectionController();

            var actionResult = SC.GetScriptSection(id);

            var response = actionResult as OkNegotiatedContentResult<ScriptSection>;
            //Assert.IsNotNull(response);

            if (response != null)
            {
                ScriptSection theSection = response.Content;

                string qincluded = "";

                bool critItems = false;

                CriticalElements Cel = GetCriticalElements(theSection, out critItems);


                List<SectionViewLayout> SectionLayout = new List<SectionViewLayout>();

                foreach (ScriptSectionLayout sectionitem in theSection.ScriptSectionLayouts)
                {
                    if (sectionitem.ShowWhen != 0)
                    {
                        ClauseEvaluator CE = new ClauseEvaluator(HttpContext.Session);
                        bool continueSection = CE.EvaluateClause(sectionitem.ShowWhen.ToString(), Convert.ToInt32(sectionitem.ShowIf));
                        if (continueSection)
                        {
                            SectionViewLayout SVL = new SectionViewLayout();
                            SVL.SectionLayout = sectionitem;


                            if (sectionitem.ElementType == "Question")
                            {
                                qincluded = qincluded == "" ? sectionitem.ElementID.ToString() : qincluded + "," + sectionitem.ElementID.ToString();

                                if (Cel.CriticalItems)
                                {
                                    if (Cel.CriticalQuestions.Contains(sectionitem.ElementID.ToString()))
                                    {
                                        SVL.CriticalElement = true;
                                    }

                                }


                            }
                            else if (sectionitem.ElementType == "OrderItem" && critItems)
                            {
                                SVL.CriticalElement = true;

                            }
                            else
                            {
                                SVL.CriticalElement = false;

                            }
                            SectionLayout.Add(SVL);

                        }

                    }
                    else
                    {

                        SectionViewLayout SVL2 = new SectionViewLayout();
                        SVL2.SectionLayout = sectionitem;

                        if (sectionitem.ElementType == "Question")
                        {
                            qincluded = qincluded == "" ? sectionitem.ElementID.ToString() : qincluded + "," + sectionitem.ElementID.ToString();

                            if (Cel.CriticalItems)
                            {
                                if (Cel.CriticalQuestions.Contains(sectionitem.ElementID.ToString()))
                                {
                                    SVL2.CriticalElement = true;
                                }

                            }

                        }
                        else if (sectionitem.ElementType == "OrderItem" && critItems)
                        {
                            SVL2.CriticalElement = true;

                        }
                        else
                        {
                            SVL2.CriticalElement = false;
                        }
                        SectionLayout.Add(SVL2);

                    }

                }

                QuestionController QuestController = new QuestionController();
                QuestionLayout questionLayout = null;
                foreach (SectionViewLayout layout in SectionLayout)
                {

                    switch (layout.SectionLayout.ElementType)
                    {
                        case "Question":

                            questionLayout = QuestController.RenderQuestionParts((decimal)layout.SectionLayout.ElementID, layout.CriticalElement, ControllerContext);

                            questionLayout.QuestionHTML = questionLayout.QuestionHTML.Replace("\r", "");
                            questionLayout.QuestionHTML = questionLayout.QuestionHTML.Replace("\n", "");
                            questionLayout.QuestionHTML = questionLayout.QuestionHTML.Replace(@"""", @"\""");
                            questionLayout.QuestionHTML = questionLayout.QuestionHTML.Replace("</script>", string.Format("\" + {0} + \"", "unescape('%3C/script%3E')"));
                            questionLayout.QuestionLabelText = questionLayout.QuestionLabelText.Replace("\r", "");
                            questionLayout.QuestionLabelText = questionLayout.QuestionLabelText.Replace("\n", "");
                            questionLayout.QuestionLabelText = questionLayout.QuestionLabelText.Replace(@"""", @"\""");
                            layout.QuestionLayout = questionLayout;
                            break;

                        case "Questionare":

                            QuestionareController QC = new QuestionareController();
                            questionLayout = new QuestionLayout();
                            //questionLayout.QuestionLabelText = "Questionare";
                            questionLayout.QuestionHTML = QC.RenderQuestionParts((decimal)layout.SectionLayout.ElementID, Cel, ControllerContext);
                            questionLayout.QuestionHTML = questionLayout.QuestionHTML.Replace("\r", "");
                            questionLayout.QuestionHTML = questionLayout.QuestionHTML.Replace("\n", "");
                            questionLayout.QuestionHTML = questionLayout.QuestionHTML.Replace(@"""", @"\""");
                            layout.QuestionLayout = questionLayout;
                            break;

                        case "ScriptLayout":

                            ScriptLayoutController SLC = new ScriptLayoutController();
                            layout.ScriptLayout = SLC.Render(layout.SectionLayout.ElementID, Cel, ControllerContext);
                            layout.ScriptLayout = HttpUtility.HtmlDecode(layout.ScriptLayout);
                            layout.ScriptLayout = layout.ScriptLayout.Replace("\r", "");
                            layout.ScriptLayout = layout.ScriptLayout.Replace("\n", "");
                            layout.ScriptLayout = layout.ScriptLayout.Replace(@"""", @"\""");
                            layout.ScriptLayout = layout.ScriptLayout.Replace(@"\\", @"\");
                            layout.ScriptLayout = layout.ScriptLayout.Replace("</script>", string.Format("\" + {0} + \"", "unescape('%3C/script%3E')"));
                            layout.ScriptLayout = layout.ScriptLayout.Replace("'s", "&apos;s");
                            break;
                        case "Action":

                            ActionController AC = new ActionController();
                            var actResult = AC.GetScriptAction(Convert.ToDecimal(layout.SectionLayout.ElementID));
                            var actResponse = actResult as OkNegotiatedContentResult<ScriptAction>;
                            ScriptAction SA = (ScriptAction)actResponse.Content;
                            ActionLayout AL = new ActionLayout();
                            AL.actionId = SA.ScriptActionID.ToString();
                            AL.actionDisplay = SA.ActionDesc;
                            AL.actionName = SA.ActionName;
                            layout.ActionLayout = AL;
                            break;
                        case "Html":
                            TextHTMLController TextHC = new TextHTMLController();
                            layout.TextHTMLLayout = TextHC.RenderHtml(Convert.ToInt32(layout.SectionLayout.ElementID), ControllerContext);
                            layout.TextHTMLLayout = HttpUtility.HtmlDecode(layout.TextHTMLLayout);
                            layout.TextHTMLLayout = layout.TextHTMLLayout.Replace("\r", "");
                            layout.TextHTMLLayout = layout.TextHTMLLayout.Replace("\n", "");
                            layout.TextHTMLLayout = layout.TextHTMLLayout.Replace(@"""", @"\""");
                            layout.TextHTMLLayout = layout.TextHTMLLayout.Replace(@"\\", @"\");
                            layout.TextHTMLLayout = layout.TextHTMLLayout.Replace("</script>", string.Format("\" + {0} + \"", "unescape('%3C/script%3E')"));
                            layout.TextHTMLLayout = layout.TextHTMLLayout.Replace("'s", "&apos;s");
                            break;
                        case "TextHTML":
                            TextHTMLController THC = new TextHTMLController();
                            layout.TextHTMLLayout = THC.Render(Convert.ToInt32(layout.SectionLayout.ElementID), ControllerContext);
                            layout.TextHTMLLayout = HttpUtility.HtmlDecode(layout.TextHTMLLayout);
                            layout.TextHTMLLayout = layout.TextHTMLLayout.Replace("\r", "");
                            layout.TextHTMLLayout = layout.TextHTMLLayout.Replace("\n", "");
                            layout.TextHTMLLayout = layout.TextHTMLLayout.Replace(@"""", @"\""");
                            layout.TextHTMLLayout = layout.TextHTMLLayout.Replace(@"\\", @"\");
                            layout.TextHTMLLayout = layout.TextHTMLLayout.Replace("</script>", string.Format("\" + {0} + \"", "unescape('%3C/script%3E')"));
                            layout.TextHTMLLayout = layout.TextHTMLLayout.Replace("'s", "&apos;s");
                            break;
                        case "OrderCart":
                            OrderCartController OCC = new OrderCartController();
                            layout.OrderCartLayout = OCC.Render(ControllerContext);
                            layout.OrderCartLayout = HttpUtility.HtmlDecode(layout.OrderCartLayout);
                            layout.OrderCartLayout = layout.OrderCartLayout.Replace("\r", "");
                            layout.OrderCartLayout = layout.OrderCartLayout.Replace("\n", "");
                            layout.OrderCartLayout = layout.OrderCartLayout.Replace(@"""", @"\""");
                            layout.OrderCartLayout = layout.OrderCartLayout.Replace(@"\\", @"\");
                            layout.OrderCartLayout = layout.OrderCartLayout.Replace("</script>", string.Format("\" + {0} + \"", "unescape('%3C/script%3E')"));
                            layout.OrderCartLayout = layout.OrderCartLayout.Replace("'s", "&apos;s");
                            break;
                        case "DataView":
                            DataViewController DVC = new DataViewController();
                            layout.DataViewLayout = DVC.Render(Convert.ToInt32(layout.SectionLayout.ElementID), ControllerContext);
                            layout.DataViewLayout = HttpUtility.HtmlDecode(layout.DataViewLayout);
                            layout.DataViewLayout = layout.DataViewLayout.Replace("\r", "");
                            layout.DataViewLayout = layout.DataViewLayout.Replace("\n", "");
                            layout.DataViewLayout = layout.DataViewLayout.Replace(@"""", @"\""");
                            layout.DataViewLayout = layout.DataViewLayout.Replace(@"\\", @"\");
                            layout.DataViewLayout = layout.DataViewLayout.Replace("</script>", string.Format("\" + {0} + \"", "unescape('%3C/script%3E')"));
                            layout.DataViewLayout = layout.DataViewLayout.Replace("'s", "&apos;s");
                            break;
                        case "ItemSelector":
                            OrderItemController OIC = new OrderItemController();
                            layout.OrderItemLayout = OIC.Render(Convert.ToInt32(layout.SectionLayout.ElementID), layout.CriticalElement, ControllerContext);
                            layout.OrderItemLayout = layout.OrderItemLayout.Replace("\r", "");
                            layout.OrderItemLayout = layout.OrderItemLayout.Replace("\n", "");
                            layout.OrderItemLayout = layout.OrderItemLayout.Replace(@"""", @"\""");
                            layout.OrderItemLayout = layout.OrderItemLayout.Replace("</script>", string.Format("\" + {0} + \"", "unescape('%3C/script%3E')"));
                            break;
                        case "Link":
                            LinkController LC = new LinkController();
                            layout.LinkLayout = LC.Render(Convert.ToInt32(layout.SectionLayout.ElementID), ControllerContext);
                            layout.LinkLayout = layout.LinkLayout.Replace("\r", "");
                            layout.LinkLayout = layout.LinkLayout.Replace("\n", "");
                            layout.LinkLayout = layout.LinkLayout.Replace(@"""", @"\""");
                            layout.LinkLayout = layout.LinkLayout.Replace("</script>", string.Format("\" + {0} + \"", "unescape('%3C/script%3E')"));
                            break;
                        case "LinkGroup":
                            LinkGroupController LGC = new LinkGroupController();
                            layout.LinkGroupLayout = LGC.Render(Convert.ToInt32(layout.SectionLayout.ElementID), ControllerContext);
                            layout.LinkGroupLayout = layout.LinkGroupLayout.Replace("\r", "");
                            layout.LinkGroupLayout = layout.LinkGroupLayout.Replace("\n", "");
                            layout.LinkGroupLayout = layout.LinkGroupLayout.Replace(@"""", @"\""");
                            layout.LinkGroupLayout = layout.LinkGroupLayout.Replace("</script>", string.Format("\" + {0} + \"", "unescape('%3C/script%3E')"));
                            break;
                        case "Image":
                            ImageController image = new ImageController();
                            layout.ImageLayout = image.Render(Convert.ToInt32(layout.SectionLayout.ElementID), ControllerContext);
                            layout.ImageLayout = layout.ImageLayout.Replace("\r", "");
                            layout.ImageLayout = layout.ImageLayout.Replace("\n", "");
                            layout.ImageLayout = layout.ImageLayout.Replace(@"""", @"\""");
                            layout.ImageLayout = layout.ImageLayout.Replace(@"\\", @"\");
                            break;
                        default:
                            break;
                    }
                }
                List<string> PopUps = new List<string>();

                ViewBag.ExternalPages = PopUps;
                ViewBag.Section = theSection.ScriptSectionID + " - " + theSection.SectionName;
                ViewBag.QuestionList = qincluded;

                string viewTemplate = "_SectionLayout?View";

                if (Session["layout"] != null)
                    viewTemplate = Session["layout"].ToString();
                else
                    viewTemplate = "SectionPreview";

                if (this.Request.Path.Contains("/Section/"))
                    return View(viewTemplate, SectionLayout);
                else
                    return View(viewTemplate, SectionLayout);
            }
            else
            {
                return null;
            }
        }

        [ActionName("DisplayByName")]
        public ActionResult Preview(string id)
        {
            ScreenViewer.API.SectionController SC = new API.SectionController();

            var actionResult = SC.GetScriptSection(id);

            var response = actionResult as OkNegotiatedContentResult<ScriptSection>;
            //Assert.IsNotNull(response);

            if (response != null)
            {
                ScriptSection theSection = response.Content;

                string qincluded = "";

                bool critItems = false;

                CriticalElements Cel = GetCriticalElements(theSection, out critItems);


                List<SectionViewLayout> SectionLayout = new List<SectionViewLayout>();

                foreach (ScriptSectionLayout sectionitem in theSection.ScriptSectionLayouts)
                {
                    if (sectionitem.ShowWhen != 0)
                    {
                        ClauseEvaluator CE = new ClauseEvaluator(HttpContext.Session);
                        bool continueSection = CE.EvaluateClause(sectionitem.ShowWhen.ToString(), Convert.ToInt32(sectionitem.ShowIf));
                        if (continueSection)
                        {
                            SectionViewLayout SVL = new SectionViewLayout();
                            SVL.SectionLayout = sectionitem;


                            if (sectionitem.ElementType == "Question")
                            {
                                qincluded = qincluded == "" ? sectionitem.ElementID.ToString() : qincluded + "," + sectionitem.ElementID.ToString();

                                if (Cel.CriticalItems)
                                {
                                    if (Cel.CriticalQuestions.Contains(sectionitem.ElementID.ToString()))
                                    {
                                        SVL.CriticalElement = true;
                                    }

                                }


                            }
                            else if (sectionitem.ElementType == "OrderItem" && critItems)
                            {
                                SVL.CriticalElement = true;

                            }
                            else
                            {
                                SVL.CriticalElement = false;

                            }
                            SectionLayout.Add(SVL);

                        }

                    }
                    else
                    {

                        SectionViewLayout SVL2 = new SectionViewLayout();
                        SVL2.SectionLayout = sectionitem;

                        if (sectionitem.ElementType == "Question")
                        {
                            qincluded = qincluded == "" ? sectionitem.ElementID.ToString() : qincluded + "," + sectionitem.ElementID.ToString();

                            if (Cel.CriticalItems)
                            {
                                if (Cel.CriticalQuestions.Contains(sectionitem.ElementID.ToString()))
                                {
                                    SVL2.CriticalElement = true;
                                }

                            }

                        }
                        else if (sectionitem.ElementType == "OrderItem" && critItems)
                        {
                            SVL2.CriticalElement = true;

                        }
                        else
                        {
                            SVL2.CriticalElement = false;
                        }
                        SectionLayout.Add(SVL2);

                    }

                }

                QuestionController QuestController = new QuestionController();
                QuestionLayout questionLayout = null;
                foreach (SectionViewLayout layout in SectionLayout)
                {

                    switch (layout.SectionLayout.ElementType)
                    {
                        case "Question":

                            questionLayout = QuestController.RenderQuestionParts((decimal)layout.SectionLayout.ElementID, layout.CriticalElement, ControllerContext);

                            questionLayout.QuestionHTML = questionLayout.QuestionHTML.Replace("\r", "");
                            questionLayout.QuestionHTML = questionLayout.QuestionHTML.Replace("\n", "");
                            questionLayout.QuestionHTML = questionLayout.QuestionHTML.Replace(@"""", @"\""");
                            questionLayout.QuestionHTML = questionLayout.QuestionHTML.Replace("</script>", string.Format("\" + {0} + \"", "unescape('%3C/script%3E')"));
                            questionLayout.QuestionLabelText = questionLayout.QuestionLabelText.Replace("\r", "");
                            questionLayout.QuestionLabelText = questionLayout.QuestionLabelText.Replace("\n", "");
                            questionLayout.QuestionLabelText = questionLayout.QuestionLabelText.Replace(@"""", @"\""");
                            layout.QuestionLayout = questionLayout;
                            break;

                        case "Questionare":

                            QuestionareController QC = new QuestionareController();
                            questionLayout = new QuestionLayout();
                            //questionLayout.QuestionLabelText = "Questionare";
                            questionLayout.QuestionHTML = QC.RenderQuestionParts((decimal)layout.SectionLayout.ElementID, Cel, ControllerContext);
                            questionLayout.QuestionHTML = questionLayout.QuestionHTML.Replace("\r", "");
                            questionLayout.QuestionHTML = questionLayout.QuestionHTML.Replace("\n", "");
                            questionLayout.QuestionHTML = questionLayout.QuestionHTML.Replace(@"""", @"\""");
                            layout.QuestionLayout = questionLayout;
                            break;

                        case "ScriptLayout":

                            ScriptLayoutController SLC = new ScriptLayoutController();
                            layout.ScriptLayout = SLC.Render(layout.SectionLayout.ElementID, Cel, ControllerContext);
                            layout.ScriptLayout = HttpUtility.HtmlDecode(layout.ScriptLayout);
                            layout.ScriptLayout = layout.ScriptLayout.Replace("\r", "");
                            layout.ScriptLayout = layout.ScriptLayout.Replace("\n", "");
                            layout.ScriptLayout = layout.ScriptLayout.Replace(@"""", @"\""");
                            layout.ScriptLayout = layout.ScriptLayout.Replace(@"\\", @"\");
                            layout.ScriptLayout = layout.ScriptLayout.Replace("</script>", string.Format("\" + {0} + \"", "unescape('%3C/script%3E')"));
                            layout.ScriptLayout = layout.ScriptLayout.Replace("'s", "&apos;s");
                            break;
                        case "Action":

                            ActionController AC = new ActionController();
                            var actResult = AC.GetScriptAction(Convert.ToDecimal(layout.SectionLayout.ElementID));
                            var actResponse = actResult as OkNegotiatedContentResult<ScriptAction>;
                            ScriptAction SA = (ScriptAction)actResponse.Content;
                            ActionLayout AL = new ActionLayout();
                            AL.actionId = SA.ScriptActionID.ToString();
                            AL.actionDisplay = SA.ActionDesc;
                            AL.actionName = SA.ActionName;
                            layout.ActionLayout = AL;
                            break;
                        case "Html":
                            TextHTMLController TextHC = new TextHTMLController();
                            layout.TextHTMLLayout = TextHC.RenderHtml(Convert.ToInt32(layout.SectionLayout.ElementID), ControllerContext);
                            layout.TextHTMLLayout = HttpUtility.HtmlDecode(layout.TextHTMLLayout);
                            layout.TextHTMLLayout = layout.TextHTMLLayout.Replace("\r", "");
                            layout.TextHTMLLayout = layout.TextHTMLLayout.Replace("\n", "");
                            layout.TextHTMLLayout = layout.TextHTMLLayout.Replace(@"""", @"\""");
                            layout.TextHTMLLayout = layout.TextHTMLLayout.Replace(@"\\", @"\");
                            layout.TextHTMLLayout = layout.TextHTMLLayout.Replace("</script>", string.Format("\" + {0} + \"", "unescape('%3C/script%3E')"));
                            layout.TextHTMLLayout = layout.TextHTMLLayout.Replace("'s", "&apos;s");
                            break;
                        case "TextHTML":
                            TextHTMLController THC = new TextHTMLController();
                            layout.TextHTMLLayout = THC.Render(Convert.ToInt32(layout.SectionLayout.ElementID), ControllerContext);
                            layout.TextHTMLLayout = HttpUtility.HtmlDecode(layout.TextHTMLLayout);
                            layout.TextHTMLLayout = layout.TextHTMLLayout.Replace("\r", "");
                            layout.TextHTMLLayout = layout.TextHTMLLayout.Replace("\n", "");
                            layout.TextHTMLLayout = layout.TextHTMLLayout.Replace(@"""", @"\""");
                            layout.TextHTMLLayout = layout.TextHTMLLayout.Replace(@"\\", @"\");
                            layout.TextHTMLLayout = layout.TextHTMLLayout.Replace("</script>", string.Format("\" + {0} + \"", "unescape('%3C/script%3E')"));
                            layout.TextHTMLLayout = layout.TextHTMLLayout.Replace("'s", "&apos;s");
                            break;
                        case "OrderCart":
                            OrderCartController OCC = new OrderCartController();
                            layout.OrderCartLayout = OCC.Render(ControllerContext);
                            layout.OrderCartLayout = HttpUtility.HtmlDecode(layout.OrderCartLayout);
                            layout.OrderCartLayout = layout.OrderCartLayout.Replace("\r", "");
                            layout.OrderCartLayout = layout.OrderCartLayout.Replace("\n", "");
                            layout.OrderCartLayout = layout.OrderCartLayout.Replace(@"""", @"\""");
                            layout.OrderCartLayout = layout.OrderCartLayout.Replace(@"\\", @"\");
                            layout.OrderCartLayout = layout.OrderCartLayout.Replace("</script>", string.Format("\" + {0} + \"", "unescape('%3C/script%3E')"));
                            layout.OrderCartLayout = layout.OrderCartLayout.Replace("'s", "&apos;s");
                            break;
                        case "DataView":
                            DataViewController DVC = new DataViewController();
                            layout.DataViewLayout = DVC.Render(Convert.ToInt32(layout.SectionLayout.ElementID), ControllerContext);
                            layout.DataViewLayout = HttpUtility.HtmlDecode(layout.DataViewLayout);
                            layout.DataViewLayout = layout.DataViewLayout.Replace("\r", "");
                            layout.DataViewLayout = layout.DataViewLayout.Replace("\n", "");
                            layout.DataViewLayout = layout.DataViewLayout.Replace(@"""", @"\""");
                            layout.DataViewLayout = layout.DataViewLayout.Replace(@"\\", @"\");
                            layout.DataViewLayout = layout.DataViewLayout.Replace("</script>", string.Format("\" + {0} + \"", "unescape('%3C/script%3E')"));
                            layout.DataViewLayout = layout.DataViewLayout.Replace("'s", "&apos;s");
                            break;
                        case "ItemSelector":
                            OrderItemController OIC = new OrderItemController();
                            layout.OrderItemLayout = OIC.Render(Convert.ToInt32(layout.SectionLayout.ElementID), layout.CriticalElement, ControllerContext);
                            layout.OrderItemLayout = layout.OrderItemLayout.Replace("\r", "");
                            layout.OrderItemLayout = layout.OrderItemLayout.Replace("\n", "");
                            layout.OrderItemLayout = layout.OrderItemLayout.Replace(@"""", @"\""");
                            layout.OrderItemLayout = layout.OrderItemLayout.Replace("</script>", string.Format("\" + {0} + \"", "unescape('%3C/script%3E')"));
                            break;
                        case "Link":
                            LinkController LC = new LinkController();
                            layout.LinkLayout = LC.Render(Convert.ToInt32(layout.SectionLayout.ElementID), ControllerContext);
                            layout.LinkLayout = layout.LinkLayout.Replace("\r", "");
                            layout.LinkLayout = layout.LinkLayout.Replace("\n", "");
                            layout.LinkLayout = layout.LinkLayout.Replace(@"""", @"\""");
                            layout.LinkLayout = layout.LinkLayout.Replace("</script>", string.Format("\" + {0} + \"", "unescape('%3C/script%3E')"));
                            break;
                        case "LinkGroup":
                            LinkGroupController LGC = new LinkGroupController();
                            layout.LinkGroupLayout = LGC.Render(Convert.ToInt32(layout.SectionLayout.ElementID), ControllerContext);
                            layout.LinkGroupLayout = layout.LinkGroupLayout.Replace("\r", "");
                            layout.LinkGroupLayout = layout.LinkGroupLayout.Replace("\n", "");
                            layout.LinkGroupLayout = layout.LinkGroupLayout.Replace(@"""", @"\""");
                            layout.LinkGroupLayout = layout.LinkGroupLayout.Replace("</script>", string.Format("\" + {0} + \"", "unescape('%3C/script%3E')"));
                            
                            break;
                        default:
                            break;
                    }
                }
                List<string> PopUps = new List<string>();

                ViewBag.ExternalPages = PopUps;
                ViewBag.Section = theSection.ScriptSectionID + " - " + theSection.SectionName;
                ViewBag.QuestionList = qincluded;
               
                string viewTemplate = "_SectionLayout?View";

                if (Session["layout"] != null)
                    viewTemplate = Session["layout"].ToString();
                else
                    viewTemplate = "SectionPreview";

                if (this.Request.Path.Contains("/Section/"))
                    return View(viewTemplate, SectionLayout);
                else
                    return View(viewTemplate, SectionLayout);
            }
            else
            {
                return null;
            }
        }


        private CriticalElements GetCriticalElements(ScriptSection ssect, out bool criticalItems)
        {
            CriticalElements CE = new CriticalElements();
            List<string> QStrings = new List<string>();
            ClauseController CC = new ClauseController();
            bool CriticalItems = false;
            foreach (ScriptSectionLayout ScrEl in ssect.ScriptSectionLayouts)
            {
                if (ScrEl.ShowWhen != null)
                {

                    //ScriptClause SC = ScriptDataAccess.GetClauseByID(ScrEl.ShowWhen, ScriptDBConnField);
                   
                    string Results = CC.GetClauseResult(Convert.ToInt32(ScrEl.ShowWhen));

                    bool critels = CC.GetClauseItemResult(Convert.ToInt32(ScrEl.ShowWhen));
                    if (critels)
                    {
                        CriticalItems = true;
                    }
                    if (Results != "")
                    {
                        string[] elestuff = System.Text.RegularExpressions.Regex.Split(Results, ",");
                        CE.CriticalItems = true;
                        bool foundit = false;
                        for (int i = 1; i < elestuff.Length; i++)
                        {
                            foundit = false;
                            foreach (object str in QStrings)
                            {
                                string qid = (string)str;
                                if (qid == elestuff[i])
                                {
                                    foundit = true;
                                    break;
                                }
                            }
                            if (foundit == false)
                            {
                                QStrings.Add(elestuff[i]);
                            }
                        }

                    }
                }
                if (ScrEl.ElementType ==  "Questionare")
                {
                          ScreenViewer.API.Elements.QuestionareController QC = new API.Elements.QuestionareController();
                          var actionResult = QC.GetScriptQuestionare((decimal)ScrEl.ElementID);
                          var response = actionResult as OkNegotiatedContentResult<ScriptQuestionare>;
     
                          ScriptQuestionare theQuestionare = response.Content;


                    foreach (ScriptQuestionareDetail QE in theQuestionare.ScriptQuestionareDetails)
                    {
                            ScreenViewer.API.Elements.QuestionController QC2 = new API.Elements.QuestionController();
                            var qactionResult = QC2.GetScriptQuestion((decimal)QE.ScriptQuestionID);
                            var response2 = qactionResult as OkNegotiatedContentResult<Data.ScriptQuestion>;
                            ScriptQuestion theQuestion = response2.Content;
                        
                        if (QE.ShowWhen > 0)
                        {
                            string Results = CC.GetClauseResult((Int32)QE.ShowWhen);
                            bool critels = CC.GetClauseItemResult((Int32)QE.ShowWhen);
                            if (critels)
                            {
                                CriticalItems = true;
                            }

                            if (Results != "")
                            {
                                string[] elestuff = System.Text.RegularExpressions.Regex.Split(Results, ",");
                                CE.CriticalItems = true;
                                bool foundit = false;
                                for (int i = 1; i < elestuff.Length; i++)
                                {

                                    foreach (object str in QStrings)
                                    {
                                        string qid = (string)str;
                                        if (qid == elestuff[i])
                                        {
                                            foundit = true;
                                            break;
                                        }
                                    }
                                    if (foundit == false)
                                    {
                                        QStrings.Add(elestuff[i]);
                                    }
                                }

                            }
                        }

                    }

                }

            }

            CE.CriticalQuestions = QStrings;
            criticalItems = CriticalItems;
            return CE;

        }

        private void FireActions(string actionlist)
        {
            if (actionlist != null && actionlist != "" && actionlist != "::")
            {

                string[] actions = Regex.Split(actionlist, ",");
                //Array.Sort(actions, StringComparer.InvariantCulture);

                foreach (string action in actions)
                {
                    string actionid = Regex.Split(action, "::")[0];
                    string actiontext = Regex.Split(action, "::")[1];

                    ScreenViewer.API.Elements.ActionController WAC = new API.Elements.ActionController();
                    var actionResult = WAC.GetScriptAction(Convert.ToDecimal(actionid));

                    ScriptAction scriptAction = null;
                    if (actionResult != null)
                    {
                        var response = actionResult as OkNegotiatedContentResult<ScriptAction>;
                        scriptAction = response.Content;
                    }
                    if (scriptAction != null)
                    {
                        switch (scriptAction.ActionType)
                        {
                            case "APIGET":
                                WorkflowHelper.APIGET(HttpContext.Session, scriptAction.ActionXML);
                                break;
                            case "APIPOST":
                                WorkflowHelper.APIPOST(HttpContext.Session, scriptAction.ActionXML);
                                break;
                            case "APIPUT":
                                WorkflowHelper.APIPUT(HttpContext.Session, scriptAction.ActionXML);
                                break;
                            case "ClearCallData":
                                SessionManager.ClearSessionData(HttpContext.Session);
                                break;
                            case "ClearMenu":
                                SessionManager.ClearMenu(HttpContext.Session);
                                break;
                            case "ClearHistory":
                                SessionManager.ClearWorkflowHistory(HttpContext.Session);
                                break;
                            case "ClearCart":
                                SessionManager.ClearOrderCart(HttpContext.Session);
                                break;
                            case "SaveProgress":
                                if (WorkflowHelper.SaveProgress(HttpContext.Session))
                                    ViewBag.SavedMessage = "Saved Successfully";
                                else
                                    ViewBag.SavedMessage = "Unable to Saved";

                                break;
                            case "SendText":
                                WorkflowHelper.SendText(HttpContext.Session, scriptAction.ActionXML);

                                break;

                            case "DispositionCall–Sale":
                                WorkflowHelper.DispositionSale(HttpContext.Session, scriptAction.ActionXML);
                                break;
                            case "DispositionCall-NoSale":
                                WorkflowHelper.DispositionNoSale(HttpContext.Session, scriptAction.ActionXML);
                                break;
                            case "Set Question Value":
                                WorkflowHelper.SetQuestionValue(HttpContext.Session, scriptAction.ActionXML);
                                break;
                            case "Set Question Text":
                                WorkflowHelper.SetQuestionText(HttpContext.Session, scriptAction.ActionXML);
                                break;
                            case "Set Parameter":
                                WorkflowHelper.SetParameter(HttpContext.Session, scriptAction.ActionXML);
                                break;

                            case "Set Item Key":
                                WorkflowHelper.SetItemKey(HttpContext.Session, scriptAction.ActionXML);
                                break;
                            case "Remove Item Key":
                                WorkflowHelper.RemoveItemKey(HttpContext.Session, scriptAction.ActionXML);
                                break;
                            case "LoadLead":
                                WorkflowHelper.LoadCRMLead(HttpContext.Session, scriptAction.ActionXML);
                                break;

                            case "validateKeyedItems":
                                Models.itemUtilities iu = new itemUtilities();
                                iu.CheckItemsHaveValidKey(HttpContext.Session);
                                break;
                            case "SaveVariables":
                                WorkflowHelper.SaveContactVariables(HttpContext.Session);
                                break;
                            case "SaveContactNavigation":
                                WorkflowHelper.SaveContactNavigation(HttpContext.Session);
                                break;
                            case "LoadPreviousCallData":
                                WorkflowHelper.RetrievePreviousCall(HttpContext.Session);

                                if (!string.IsNullOrEmpty(SessionManager.GetScriptParameterByKey("Critical", HttpContext.Session)) && SessionManager.GetScriptParameterByKey("Critical", HttpContext.Session) == "Yes")
                                    ViewBag.Critical = "Customer spent over 5 minutes in system.  Be empathetic and cautious with assistance to prevent any further frustration";

                                break;
                            case "Email":
                                WorkflowHelper.Email(HttpContext.Session, scriptAction.ActionXML);
                                break;
                            case "Insert IDR Queue":
                                WorkflowHelper.InsertIDRQueue(HttpContext.Session, scriptAction.ActionXML);
                                break;
                            case "Update IDR Queue Status":
                                WorkflowHelper.UpdateIDRQueue(HttpContext.Session, scriptAction.ActionXML);
                                break;
                            case "UpdateObject":
                                WorkflowHelper.UpdateObject(HttpContext.Session, scriptAction.ActionXML);
                                break;
                            case "SaveLead":
                                WorkflowHelper.SaveCRMLead(HttpContext.Session);
                                break;
                            case "GeneratePDF":
                                WorkflowHelper.GeneratePDF(HttpContext.Session, scriptAction.ActionXML);
                                break;
                        }
                    }
                }
            }
        }

        //[ActionName("DefaultWithLayout")]
        //public ActionResult Display(Decimal Id, int  idx = 0)
        //{
        //    Session["layout"] = "_SectionMenuView";
        //    ActionResult result = Display(Id);
        //    WFNodeInfo nextNode = ScreenViewer.SessionControl.SessionManager.GetNextNode(HttpContext.Session);
        //    nextNode.nodeName = Id.ToString();
        //    ViewBag.NextNode = nextNode;
        //    Session["layout"] = null;
        //    return result;
        //}

    }
}