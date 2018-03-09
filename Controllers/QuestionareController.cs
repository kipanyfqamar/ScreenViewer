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
    public class QuestionareController : Controller
    {
        //
        // GET: /Questionare/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Display(string id)
        {
            ScreenViewer.API.Elements.QuestionareController QC = new API.Elements.QuestionareController();
            var actionResult = QC.GetScriptQuestionare(Convert.ToDecimal(id));
            var response = actionResult as OkNegotiatedContentResult<Data.ScriptQuestionare>;
            QuestionController QuestController = new QuestionController();
            
            if (response != null)
            {
                Data.ScriptQuestionare theQuestionare = response.Content;
                List<QuestionLayout> QuestionLayout = new List<QuestionLayout>();
                ViewBag.QID = theQuestionare.ScriptQuestionareID;
                string qincluded = "";

                //StringBuilder SBstyles = new StringBuilder("");
                   
                //if (theQuestionare.BackColor != "Not Selected")
                //{
                //    SBstyles.Append("background-color:" + theQuestionare.BackColor + "; ");
                //}                
                //if (theQuestionare.FontColor != "Not Selected")
                //{
                //    SBstyles.Append("color:" + theQuestionare.FontColor + "; ");
                //}   
                //if (theQuestionare.BorderColor != "Not Selected")
                //{
                //    SBstyles.Append("border-color:" + theQuestionare.BorderColor + "; ");
                //}   

                //SBstyles.Append(theQuestionare.FontFamily + " ");
                //SBstyles.Append("font-size:" + theQuestionare.FontSize);

                //@ViewBag.Tablestyle = SBstyles.ToString();

                foreach (Data.ScriptQuestionareDetail questionitem in theQuestionare.ScriptQuestionareDetails)
                {
                    if (questionitem.ShowWhen > 0)
                    {


                        ClauseEvaluator CE = new ClauseEvaluator(HttpContext.Session);
                        bool continueSection = CE.EvaluateClause(questionitem.ShowWhen.ToString(), Convert.ToInt32(questionitem.Showif));
                        if (continueSection)
                        {

                            QuestionLayout.Add( QuestController.RenderQuestionParts((decimal)questionitem.ScriptQuestionID, false, ControllerContext));
                            qincluded = qincluded == "" ? questionitem.ScriptQuestionID.ToString() : qincluded + "," + questionitem.ScriptQuestionID.ToString();

                        }

                    }
                    else
                    {
                        QuestionLayout.Add(QuestController.RenderQuestionParts((decimal)questionitem.ScriptQuestionID, false, ControllerContext));
                        qincluded = qincluded == "" ? questionitem.ScriptQuestionID.ToString() : qincluded + "," + questionitem.ScriptQuestionID.ToString();
                    }
                    
                }
                ViewBag.QuestionList = qincluded;
                ViewBag.QuestionLayout = QuestionLayout;
                ViewBag.Heading = theQuestionare.QuestionareDesc;

                string[] layout = theQuestionare.LayoutType.Split(':');

                if (layout[0] == "S")
                {

                    return PartialView("_QuestionareView" + layout[1] + "col", QuestionLayout);
                }
                else
                {
                    return PartialView("_QuestionareView" + layout[1] + "colover", QuestionLayout);



                }
            }
            else
            {
                return null;
            }
        }

        public string RenderQuestionParts(decimal id, ScreenViewer.Models.CriticalElements criticalelement, ControllerContext ContCont)

        {
            ScreenViewer.API.Elements.QuestionareController QC = new API.Elements.QuestionareController();
            var actionResult = QC.GetScriptQuestionare(Convert.ToDecimal(id));
            var response = actionResult as OkNegotiatedContentResult<Data.ScriptQuestionare>;
            QuestionController QuestController = new QuestionController();

            if (response != null)
            {
                Data.ScriptQuestionare theQuestionare = response.Content;
                List<QuestionLayout> QuestionLayout = new List<QuestionLayout>();
                ViewBag.QID = theQuestionare.ScriptQuestionareID;
                string qincluded = "";

                StringBuilder SBstyles = new StringBuilder("");

                //SBstyles.Append("border-spacing: 10px;border-collapse: separate;");

                //if (theQuestionare.BackColor != "Not Selected")
                //{
                //    SBstyles.Append("background-color:" + theQuestionare.BackColor + "; ");
                //}
                //if (theQuestionare.FontColor != "Not Selected")
                //{
                //    SBstyles.Append("color:" + theQuestionare.FontColor + "; ");
                //}
                //if (theQuestionare.BorderColor != "Not Selected")
                //{
                //    SBstyles.Append("border-color:" + theQuestionare.BorderColor + "; ");
                //}

                //if (theQuestionare.GridLines.Value)
                //    @ViewBag.GridLine = "1";
                //else
                //    @ViewBag.GridLine = "0";


                //SBstyles.Append(theQuestionare.FontFamily + " ");
                //SBstyles.Append("font-size:" + theQuestionare.FontSize);

                @ViewBag.Tablestyle = SBstyles.ToString();

                foreach (Data.ScriptQuestionareDetail questionitem in theQuestionare.ScriptQuestionareDetails)
                {
                    if (questionitem.ShowWhen > 0)
                    {


                        ClauseEvaluator CE = new ClauseEvaluator(ContCont.HttpContext.Session);
                        bool continueSection = CE.EvaluateClause(questionitem.ShowWhen.ToString(), Convert.ToInt32(questionitem.Showif));
                        if (continueSection)
                        {
                            bool crit = false;
                            if (criticalelement.CriticalItems)
                            {
                                if (criticalelement.CriticalQuestions.Contains(questionitem.ScriptQuestionID.ToString()))
                                {
                                    crit = true;
                                }
                            }

                            QuestionLayout.Add(QuestController.RenderQuestionParts((decimal)questionitem.ScriptQuestionID, crit, ContCont));
                            qincluded = qincluded == "" ? questionitem.ScriptQuestionID.ToString() : qincluded + "," + questionitem.ScriptQuestionID.ToString();

                        }

                    }
                    else
                    {
                        bool crit = false;
                        if (criticalelement.CriticalItems)
                        {
                            if (criticalelement.CriticalQuestions.Contains(questionitem.ScriptQuestionID.ToString()))
                            {
                                crit = true;
                            }
                        }
                        QuestionLayout.Add(QuestController.RenderQuestionParts((decimal)questionitem.ScriptQuestionID, crit, ContCont));
                        qincluded = qincluded == "" ? questionitem.ScriptQuestionID.ToString() : qincluded + "," + questionitem.ScriptQuestionID.ToString();
                    }

                }
                ViewBag.QuestionList = qincluded;
                ViewBag.QuestionCount = QuestionLayout.Count;
                ViewBag.QuestionLayout = QuestionLayout;
                ViewBag.Title = theQuestionare.QuestionareDesc;
                string[] layout = theQuestionare.LayoutType.Split(':');

                string vname = "";
                if (layout[0] == "S")
                {
                    vname = "~/Views/Questionare/_QuestionareView" + layout[1] + "col" + ".cshtml";

                }
                else
                {
                    vname = "~/Views/Questionare/_QuestionareView" + layout[1] + "colover" + ".cshtml";

                }
                return RenderHelper.RenderViewToString(ContCont, vname, QuestionLayout, ViewData);


            }
            else
            {
                return null;
            }
        }


	}
}