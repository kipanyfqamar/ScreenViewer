using System;
using System.Xml.Serialization;
using System.IO;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Web.Mvc;
using System.Web.Http.Results;
using System.Text.RegularExpressions;


namespace ScreenViewer.Controllers
{
    public class QuestionController : Controller
    {
        public const string FREE_TEXT = "F";
        public const string CONFIRM = "C";
        public const string YES_NO = "Y";
        public const string PICK_ONE = "O";
        public const string PICK_ONE_ALT = "L";
        public const string PICK_MANY = "M";
        public bool Critical = false;
        //

        class ListItems
        {
            public string DisplayValue { get; set; }
            public string ActualValue { get; set; }
         }

        
        // GET: /Question/
        public ActionResult Index()
        {
            return View();
        }

        //public ActionResult Display(decimal ID)
        //{
        //    ViewBag.QuestionID = ID;
            
        //    var pview = RenderQuestion(ID);
        //    ViewBag.PartialView = pview;
        //    //return RedirectToAction(RenderQuestion(ID));
        //    return RedirectToAction("RenderQuestion", new { ID = ID });

        //    //return View("_Question");
            
        //}
        //public ActionResult Display(decimal ID)
        //{
        //    return Display(ID, "false");
        //}
        //public ActionResult Display(decimal ID,bool criticalelement)
        //{
        //    ViewBag.QuestionID = ID;
        //    ViewBag.DisplayLabel = true;
        //    ScreenViewer.API.Elements.QuestionController QC = new API.Elements.QuestionController();
        //    var actionResult = QC.GetScriptQuestion(ID);



        //    var response = actionResult as OkNegotiatedContentResult<Data.ScriptQuestion>;
        //    //Assert.IsNotNull(response);
        //    Data.ScriptQuestion theQuestion = response.Content;
        //    ViewBag.QuestionDisplay = SPutilities.ReplaceObjectsandQuestions(HttpContext.Session, theQuestion.QuestionText,true);
        //    ViewBag.QuestionID = theQuestion.ScriptQuestionID;
        //    ViewBag.ControlName = "SPQuestion_" + theQuestion.ScriptQuestionID;
        //    if (criticalelement)
        //    {
        //        ViewBag.ControlName += "_C";
        //    }
        //    string cresp = SessionControl.SessionManager.GetQuestionResponse(ID.ToString(), HttpContext.Session);
                
        //    ViewBag.ReturnType = theQuestion.ResponseType;
        //    ViewBag.AnswerRequired = theQuestion.AnswerRequired.Value ? "required" : string.Empty;

        //    switch (theQuestion.QuestionType)
        //    {
        //        case FREE_TEXT:
        //            ViewBag.CurrentResponse = cresp;
        //            ViewBag.ValidatorRegex = null;
        //            if (theQuestion.ScriptValidatorID != null && theQuestion.ScriptValidatorID != 0)
        //            {
        //                var result = QC.GetScriptTextValidator(theQuestion.ScriptValidatorID.Value);
        //                var ValidatorResponse = result as OkNegotiatedContentResult<Data.ScriptValidator>;
        //                Data.ScriptValidator scriptTextValidator = ValidatorResponse.Content;
        //                ViewBag.ValidatorRegex = scriptTextValidator.ValidatorRegex;
        //                ViewBag.ValidatorDesc = scriptTextValidator.ValidatorDesc;
        //            }
        //            switch (theQuestion.ResponseType)
        //            {
        //                case "Date":
        //                case "DateTime":
        //                case "Time":
        //                    return PartialView("_DateTimeQuestion");
        //                 case "Number":

        //                    if (theQuestion.MinimumValue != null)
        //                    {
        //                        ViewBag.MinValue = theQuestion.MinimumValue;
        //                    }
        //                    if (theQuestion.MaximumValue != null)
        //                    {

        //                       ViewBag.MaxValue = theQuestion.MaximumValue;
        //                    }
        //                    return PartialView("_NumericQuestion");
        //                case "String":
        //                    ViewBag.MaxLength = theQuestion.MaxLength;
        //                    return PartialView("_TextQuestion");
        //            }
        //            break;
        //        case CONFIRM:
        //           ViewBag.CurrentResponse = cresp;
        //           ViewBag.Checked = cresp == "True" ? "Checked" : "";
        //            return PartialView("_ConfirmQuestion");
        //        case YES_NO:
        //            ViewBag.CurrentResponse = cresp;
        //            ViewBag.YesCheck = cresp == "Yes" ? "Checked" : "";
        //            ViewBag.NoCheck = cresp == "No" ? "Checked" : "";

        //            return PartialView("_YesNoQuestion");
        //        case PICK_ONE_ALT:
        //        case PICK_ONE:
        //            ViewBag.CurrentResponse = cresp == null ? null :cresp;
        //            if (!string.IsNullOrEmpty(theQuestion.DataObjectField))
        //            {


        //            }
        //            else
        //            {

        //                List<SelectListItem> pickList = CreateDOList(theQuestion.Choices);
        //                ViewBag.PickList = pickList;
        //                ViewBag.PickLength = pickList.Count;
        //                ViewBag.ControlID = "listquestion_" + theQuestion.ScriptQuestionID.ToString();

        //                return PartialView("_PickOneQuestion");
        //            }
        //            break;
        //        case PICK_MANY:
        //            ViewBag.CurrentResponse = cresp == null ? null : Regex.Split(cresp, ",").ToList();
        //            if (!string.IsNullOrEmpty(theQuestion.DataObjectField))
        //            {


        //            }
        //            else
        //            {
        //                List<SelectListItem> pickList = CreateDOList(theQuestion.Choices);
        //                ViewBag.PickList = pickList;
        //                ViewBag.PickLength = pickList.Count;
        //                ViewBag.ID = theQuestion.ScriptQuestionID.ToString();
        //                ViewBag.ControlID = "listquestion_" + theQuestion.ScriptQuestionID.ToString();
        //                return PartialView("_PickManyQuestion");

        //            }  
        //            break;
        //        default:
        //            break;


        //    }
        //    return Content("Blah Blah");
        //}
        public ScreenViewer.Models.Elements.QuestionLayout RenderQuestionParts(decimal ID, bool criticalelement, ControllerContext ContCont)
        {
            ScreenViewer.Models.Elements.QuestionLayout questionLayout = new Models.Elements.QuestionLayout();

            ViewBag.QuestionID = ID;
            ViewBag.DisplayLabel = false;
            ScreenViewer.API.Elements.QuestionController QC = new API.Elements.QuestionController();
            var actionResult = QC.GetScriptQuestion(ID);



            var response = actionResult as OkNegotiatedContentResult<Data.ScriptQuestion>;
            //Assert.IsNotNull(response);
            Data.ScriptQuestion theQuestion = response.Content;
            ViewBag.QuestionDisplay = SPutilities.ReplaceObjectsandQuestions(ContCont.HttpContext.Session, theQuestion.QuestionText, true);
            ViewBag.QuestionID = theQuestion.ScriptQuestionID;
            ViewBag.ControlName = "SPQuestion_" + theQuestion.ScriptQuestionID;
            if (criticalelement)
            {
                ViewBag.ControlName += "_C";
            }
            string cresp = SessionControl.SessionManager.GetQuestionResponse(ID.ToString(), ContCont.HttpContext.Session);

            if (string.IsNullOrEmpty(cresp) && !string.IsNullOrEmpty(theQuestion.KeyCodes))
            {
                if (theQuestion.KeyCodes.StartsWith("DO."))
                {
                    string[] holdKeyCode = theQuestion.KeyCodes.Split('.');
                    string dataObject = holdKeyCode[1];
                    ScreenViewer.Models.DataObjects ScriptDataObject = (ScreenViewer.Models.DataObjects)SessionControl.SessionManager.GetDataObject(ContCont.HttpContext.Session, dataObject);

                    if (ScriptDataObject != null)
                    {
                        foreach (var detail in ScriptDataObject.Details)
                        {
                            if (detail.DetailName == holdKeyCode[2] && detail.DetailValue != null)
                                cresp = detail.DetailValue.ToString();
                                
                        }
                    }
                }
            }

            ViewBag.ReturnType = theQuestion.ResponseType;

            try
            {

                ViewBag.AnswerRequired = ((bool)theQuestion.AnswerRequired) ? "required" : string.Empty;
            }
            catch
            {
                ViewBag.AnswerRequired = string.Empty;
            }
            questionLayout.QuestionLabelText = ViewBag.QuestionDisplay;//theQuestion.QuestionText;


            switch (theQuestion.QuestionType)
            {
                case FREE_TEXT:
                    ViewBag.Type = "text";
                    ViewBag.CurrentResponse = cresp;
                    ViewBag.ValidatorRegex = null;

                    if (theQuestion.QuestionDesc.Substring(theQuestion.QuestionDesc.Length - 1) == "$")
                        ViewBag.Type = "password";

                    if (theQuestion.ScriptValidatorID != null && theQuestion.ScriptValidatorID != 0)
                    {
                        var result = QC.GetScriptTextValidator(theQuestion.ScriptValidatorID.Value);
                        var ValidatorResponse = result as OkNegotiatedContentResult<Data.ScriptValidator>;
                        Data.ScriptValidator scriptTextValidator = ValidatorResponse.Content;
                        ViewBag.ValidatorRegex = scriptTextValidator.ValidatorRegex;
                        ViewBag.ValidatorDesc = scriptTextValidator.ValidatorDesc;
                    }

                    switch (theQuestion.ResponseType)
                    {
                        case "Date":
                        case "DateTime":
                        case "Time":
                            questionLayout.QuestionHTML = RenderHelper.RenderViewToString(ContCont, "~/Views/Question/_DateTimeQuestion.cshtml", theQuestion, ViewData);
                            break;
                        case "Number":

                            if (theQuestion.MinimumValue != null)
                            {
                                ViewBag.MinValue = theQuestion.MinimumValue;
                            }
                            if (theQuestion.MaximumValue != null)
                            {

                                ViewBag.MaxValue = theQuestion.MaximumValue;
                            }
                            questionLayout.QuestionHTML = RenderHelper.RenderViewToString(ContCont, "~/Views/Question/_NumericQuestion.cshtml", theQuestion, ViewData);
                            break;
                        case "String":
                            ViewBag.MaxLength = theQuestion.MaxLength;

                            questionLayout.QuestionHTML = RenderHelper.RenderViewToString(ContCont, "~/Views/Question/_TextQuestion.cshtml", theQuestion, ViewData);
                            break;

                    }
                    break;
                case CONFIRM:
                    ViewBag.CurrentResponse = cresp;
                    ViewBag.Checked = cresp == "True" ? "Checked" : "";
                    questionLayout.QuestionHTML = RenderHelper.RenderViewToString(ContCont, "~/Views/Question/_ConfirmQuestion.cshtml", theQuestion, ViewData);
                    break;


                case YES_NO:
                    ViewBag.CurrentResponse = cresp;
                    ViewBag.YesCheck = cresp == "Yes" ? "Checked" : "";
                    ViewBag.NoCheck = cresp == "No" ? "Checked" : "";

                    questionLayout.QuestionHTML = RenderHelper.RenderViewToString(ContCont, "~/Views/Question/_YesNoQuestion.cshtml", theQuestion, ViewData);
                    break;
                case PICK_ONE_ALT:
                case PICK_ONE:

                    ViewBag.CurrentResponse = cresp == null ? null : cresp;
                    ViewBag.QuestionType = theQuestion.QuestionType;
                    ViewBag.MaxLength = theQuestion.MaxLength;
                    if (!string.IsNullOrEmpty(theQuestion.DataObjectField))
                    {
                        List<string> displayItems = Regex.Split(theQuestion.DODisplayColumns, "~").ToList();
                        if (displayItems.Count > 1)
                        {
                            DataTable listtable = CreateObjectSelectList(theQuestion, ContCont.HttpContext.Session);
                            ViewBag.ValueColumn = theQuestion.DOValueColumn;
                            ViewBag.DataTable = listtable;
                            ViewBag.ControlID = "gridquestion_" + theQuestion.ScriptQuestionID.ToString();
                            ViewBag.OID = theQuestion.ScriptQuestionID.ToString();
                            questionLayout.QuestionHTML = RenderHelper.RenderViewToString(ContCont, "~/Views/Question/_ObjectSelector.cshtml", theQuestion, ViewData);

                        }
                        else
                        {
                            List<SelectListItem> pickList = CreateDobList(theQuestion, ContCont.HttpContext.Session);
                            ViewBag.PickList = pickList;
                            ViewBag.PickLength = pickList.Count;
                            ViewBag.ControlID = "listquestion_" + theQuestion.ScriptQuestionID.ToString();
                            questionLayout.QuestionHTML = RenderHelper.RenderViewToString(ContCont, "~/Views/Question/_PickOneQuestion.cshtml", theQuestion, ViewData);
                            break;
                        }

                    }
                    else if (theQuestion.ScriptLookupID != null && theQuestion.ScriptLookupID.Value != 0.0M)
                    {
                        List<SelectListItem> pickList = CreateLookupList(theQuestion, ContCont.HttpContext.Session);
                        ViewBag.PickList = pickList;
                        ViewBag.PickLength = pickList.Count;
                        ViewBag.ControlID = "listquestion_" + theQuestion.ScriptQuestionID.ToString();
                        questionLayout.QuestionHTML = RenderHelper.RenderViewToString(ContCont, "~/Views/Question/_PickOneQuestion.cshtml", theQuestion, ViewData);
                        break;
                    }
                    else
                    {

                        List<SelectListItem> pickList = CreateDOList(theQuestion.Choices);
                        ViewBag.PickList = pickList;
                        ViewBag.PickLength = pickList.Count;
                        ViewBag.ControlID = "listquestion_" + theQuestion.ScriptQuestionID.ToString();
                        questionLayout.QuestionHTML = RenderHelper.RenderViewToString(ContCont, "~/Views/Question/_PickOneQuestion.cshtml", theQuestion, ViewData);
                        break;

                    }
                    break;
                case PICK_MANY:
                    ViewBag.CurrentResponse = cresp == null ? null : Regex.Split(cresp, ",").ToList();
                    ViewBag.CurrentResponse = cresp == null ? null : Regex.Split(cresp, "~").ToList();
                    ViewBag.MaxLength = theQuestion.MaxLength;

                    //string pmResponse = string.Empty;
                    //List<string> holdList = cresp == null ? null : Regex.Split(cresp, ",").ToList();
                    //if (holdList != null && holdList.Count > 0)
                    //{
                    //    foreach (var item in holdList)
                    //    {
                    //        pmResponse = string.Format("{0}'{1}',", pmResponse, item);
                    //    }

                    //    pmResponse = pmResponse.TrimEnd(',');
                    //    ViewBag.CurrentResponse = System.Web.HttpUtility.JavaScriptStringEncode(pmResponse);
                    //}
                    //else
                    //    ViewBag.CurrentResponse = pmResponse;

                    List<string> displayItems2 = null;
                    if (theQuestion.DODisplayColumns != null)
                        displayItems2 = Regex.Split(theQuestion.DODisplayColumns, "~").ToList();

                    if (!string.IsNullOrEmpty(theQuestion.DataObjectField))
                    {

                        if (displayItems2.Count > 1)
                        {
                            DataTable listtable = CreateObjectSelectList(theQuestion, ContCont.HttpContext.Session);
                            ViewBag.DataTable = listtable;
                            ViewBag.ControlID = "gridquestion_" + theQuestion.ScriptQuestionID.ToString();
                            questionLayout.QuestionHTML = RenderHelper.RenderViewToString(ContCont, "~/Views/Question/_ObjectSelector.cshtml", theQuestion, ViewData);

                        }
                        else
                        {
                            List<SelectListItem> pickList = CreateDobList(theQuestion, ContCont.HttpContext.Session);
                            ViewBag.PickList = pickList;
                            ViewBag.PickLength = pickList.Count;
                            ViewBag.ID = theQuestion.ScriptQuestionID.ToString();
                            ViewBag.ControlID = "listquestion_" + theQuestion.ScriptQuestionID.ToString();
                            questionLayout.QuestionHTML = RenderHelper.RenderViewToString(ContCont, "~/Views/Question/_PickManyQuestion.cshtml", theQuestion, ViewData);
                        }
                    }
                    else if (theQuestion.ScriptLookupID != null && theQuestion.ScriptLookupID.Value != 0.0M)
                    {
                        List<SelectListItem> pickList = CreateLookupList(theQuestion, ContCont.HttpContext.Session);
                        ViewBag.PickList = pickList;
                        ViewBag.PickLength = pickList.Count;
                        ViewBag.ID = theQuestion.ScriptQuestionID.ToString();
                        ViewBag.ControlID = "listquestion_" + theQuestion.ScriptQuestionID.ToString();
                        questionLayout.QuestionHTML = RenderHelper.RenderViewToString(ContCont, "~/Views/Question/_PickManyQuestion.cshtml", theQuestion, ViewData);
                    }
                    else
                    {
                        List<SelectListItem> pickList = CreateDOList(theQuestion.Choices);
                        ViewBag.PickList = pickList;
                        ViewBag.PickLength = pickList.Count;
                        ViewBag.ID = theQuestion.ScriptQuestionID.ToString();
                        ViewBag.ControlID = "listquestion_" + theQuestion.ScriptQuestionID.ToString();
                        questionLayout.QuestionHTML = RenderHelper.RenderViewToString(ContCont, "~/Views/Question/_PickManyQuestion.cshtml", theQuestion, ViewData);
                        break;


                    }
                    break;
                default:
                    break;


            }
            return questionLayout;

        }


        //public string Render(decimal ID,ControllerContext ContCont)
        //{
        //    ViewBag.QuestionID = ID;

        //    ScreenViewer.API.Elements.QuestionController QC = new API.Elements.QuestionController();
        //    var actionResult = QC.GetScriptQuestion(ID);

        //    var response = actionResult as OkNegotiatedContentResult<Data.ScriptQuestion>;
        //    //Assert.IsNotNull(response);
        //    Data.ScriptQuestion theQuestion = response.Content;
        //    ViewBag.QuestionDisplay = theQuestion.QuestionText;
        //    ViewBag.QuestionID = theQuestion.ScriptQuestionID;

        //    ViewBag.ReturnType = theQuestion.ResponseType;
        //    ViewBag.AnswerRequired = theQuestion.AnswerRequired.Value ? "required" : string.Empty;

        //    switch (theQuestion.QuestionType)
        //    {
        //        case FREE_TEXT:
        //            ViewBag.ValidatorRegex = null;
        //            if (theQuestion.ScriptValidatorID != null && theQuestion.ScriptValidatorID != 0)
        //            {
        //                var result = QC.GetScriptTextValidator(theQuestion.ScriptValidatorID.Value);
        //                var ValidatorResponse = result as OkNegotiatedContentResult<Data.ScriptValidator>;
        //                Data.ScriptValidator scriptTextValidator = ValidatorResponse.Content;
        //                ViewBag.ValidatorRegex = scriptTextValidator.ValidatorRegex;
        //                ViewBag.ValidatorDesc = scriptTextValidator.ValidatorDesc;
        //            }
        //            switch (theQuestion.ResponseType)
        //            {
        //                case "Date":
        //                case "DateTime":
        //                case "Time":
        //                    return RenderHelper.RenderViewToString(ContCont, "_DateTimeQuestion", null, ViewData);

        //                case "Number":

        //                    if (theQuestion.MinimumValue != null)
        //                    {
        //                        ViewBag.MinValue = theQuestion.MinimumValue;
        //                    }
        //                    if (theQuestion.MaximumValue != null)
        //                    {

        //                        ViewBag.MaxValue = theQuestion.MaximumValue;
        //                    }
        //                    return RenderHelper.RenderViewToString(ContCont, "_NumericQuestion", null, ViewData);
        //                case "String":
        //                    ViewBag.MaxLength = theQuestion.MaxLength;
        //                     return RenderHelper.RenderViewToString(ContCont, "_TextQuestion", null, ViewData);
        //            }
        //            break;
        //        case CONFIRM:
        //            return RenderHelper.RenderViewToString(ContCont, "_ConfirmQuestion", null, ViewData);
        //        case YES_NO:
        //                     return RenderHelper.RenderViewToString(ContCont, "_YesNoQuestion", null, ViewData);
        //        case PICK_ONE:
        //            if (!string.IsNullOrEmpty(theQuestion.DataObjectField))
        //            {


        //            }
        //            else
        //            {
        //                List<SelectListItem> pickList = CreateDOList(theQuestion.Choices);

        //                //List<SelectListItem> pickList = CreateDobList(theQuestion, ContCont.HttpContext.Session);
        //                ViewBag.PickList = pickList;
        //                ViewBag.PickLength = pickList.Count;
        //                ViewBag.ControlID = "listquestion_" + theQuestion.ScriptQuestionID.ToString();

        //                string viewRendered = RenderHelper.RenderViewToString(ContCont, "_PickOneQuestion", theQuestion, ViewData);
                           

        //            }
        //            break;
        //        case PICK_MANY:
        //            if (!string.IsNullOrEmpty(theQuestion.DataObjectField))
        //            {


        //            }
        //            else
        //            {
        //                List<SelectListItem> pickList = CreateDOList(theQuestion.Choices);
        //                ViewBag.PickList = pickList;
        //                ViewBag.PickLength = pickList.Count;
        //                ViewBag.ID = theQuestion.ScriptQuestionID.ToString();
        //                ViewBag.ControlID = "listquestion_" + theQuestion.ScriptQuestionID.ToString();
        //                return RenderHelper.RenderViewToString(ContCont, "_PickManyQuestion", null, ViewData);
        //            }
        //            break;
        //        default:
        //            break;


        //    }
        //    return "";
        //}

        private List<SelectListItem> CreateDOList(String thelist)
        {

            List<SelectListItem> DDItemList = new List<SelectListItem>();
            string[] theItems = Regex.Split(thelist, "~~");
            foreach (string item in theItems)
            {
                string[] thevals = Regex.Split(item, @"\|\|");
                DDItemList.Add(new SelectListItem() { Text = thevals[0], Value = thevals[1]});
            }

            return DDItemList;

        }

        public ActionResult Grid_Create([DataSourceRequest]DataSourceRequest request, string id)
        {
            ScreenViewer.API.Elements.QuestionController QC = new API.Elements.QuestionController();
            var actionResult = QC.GetScriptQuestion(Convert.ToDecimal(id));

            ViewBag.OID = id.ToString();

            var response = actionResult as OkNegotiatedContentResult<Data.ScriptQuestion>;
            //Assert.IsNotNull(response);
            Data.ScriptQuestion theQuestion = response.Content;


            DataTable dt = SPutilities.GeneratePickDataCollection(theQuestion.DataObjectField, theQuestion.DOValueColumn, theQuestion.DODisplayColumns, HttpContext.Session);

            return Json(dt.ToDataSourceResult(request));
        }
        private List<SelectListItem> CreateDobList(Data.ScriptQuestion theQuestion, HttpSessionStateBase theSession)
        {

            List<SelectListItem> DDItemList = new List<SelectListItem>();
            List<string> theList = SPutilities.GeneratePickData1(theQuestion.DataObjectField,theQuestion.DOValueColumn,theQuestion.DODisplayColumns,theSession);
            foreach (string item in theList)
            {
                string[] thevals = Regex.Split(item, @"\|\|");
                DDItemList.Add(new SelectListItem() { Text = thevals[1], Value = thevals[0] });
            }

            return DDItemList;

        }
        private DataTable CreateObjectSelectList(Data.ScriptQuestion theQuestion, HttpSessionStateBase theSession)
        {

            DataTable dt = SPutilities.GetSelectGridCols(theQuestion.DOValueColumn, theQuestion.DODisplayColumns);
            //DataTable dt = SPutilities.GeneratePickDataCollection(theQuestion.DataObjectField, theQuestion.DOValueColumn, theQuestion.DODisplayColumns, theSession);
        
            return dt;

        }

        private List<SelectListItem> CreateLookupList(Data.ScriptQuestion theQuestion, HttpSessionStateBase theSession)
        {

            List<SelectListItem> lookupList = new List<SelectListItem>();
            ScreenViewer.API.Elements.QuestionController QC = new API.Elements.QuestionController();
            var actionResult = QC.GetScriptQuestionLookup(theQuestion.ScriptLookupID.Value);

            var response = actionResult as OkNegotiatedContentResult<Data.ScriptLookup>;

            foreach (var item in response.Content.ScriptLookupDetails)
            {
                lookupList.Add(new SelectListItem() { Text = item.LookupText, Value = item.LookupValue });
            }

            return lookupList;
        }

	}
}