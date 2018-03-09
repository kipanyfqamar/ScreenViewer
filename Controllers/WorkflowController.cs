using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Mvc;
using System.Web.Http.Results;
using Microsoft.AspNet.Identity;
using ScreenViewer.API;
using ScreenViewer.Models;
using ScreenViewer.SessionControl;
using System.Text.RegularExpressions;
using ScreenViewer.API.ExternalData;
using ScreenViewer.Models.ExternalData;
using ScreenViewer;
using ScreenViewer.Data;
using ScreenViewer.Controllers;
using Kendo.Mvc;
using Kendo.Mvc.UI;
using System.Web.Script.Serialization;

namespace ScreenViewer.Controllers
{
    public class WorkflowController : Controller
    {
        public ArrayList WFHistArray;
        public int currentNode = 0;
        public MoveDirection direction = MoveDirection.Start;
        public enum MoveDirection { Forward, Back, Current, Start };
        public List<string> PopUps = new List<string>();
        public bool FireAway = true;
        public bool IsPartialView = false;

        public struct ItemCounts
        {
            public string itemCode;
            public Int32 itemQuantity;
            public string ownerOIS;
        }

        [System.Web.Mvc.ActionName("DisplayByUniqueName")]
        public ActionResult Display(string id)
        {
            ScreenViewer.API.WorkflowController WFC = new API.WorkflowController();
            int workFlowId = WFC.GetScriptWorkflowId(id);
            return RedirectToAction("Display", "Workflow", new { id = workFlowId });
        }

        public ActionResult Display(int id)
        {
            WorkflowDisplay wfDisplay = null;
            WFNodeInfo nextNode = null;
            ScreenViewer.API.WorkflowController WFC = null;
            string error = string.Empty;

            try
            {
                wfDisplay = new WorkflowDisplay();
                WFC = new API.WorkflowController();

                if (!ScreenViewer.SignInHelper.ValidateSignIn())
                    return RedirectToAction("Login", "Account");

                wfDisplay.workflowID = id.ToString();
                wfDisplay.Layout = SessionManager.GetLayoutHTML(HttpContext.Session);
                wfDisplay.Notifications = SessionManager.GetNotificationText(HttpContext.Session);
                wfDisplay.menuHTML = SessionManager.GetMenuHTML(HttpContext.Session);
                wfDisplay.callNotes = SessionManager.GetContactNotes(HttpContext.Session);

                var actionResult = WFC.GetScriptWorkflow(id);

                if (actionResult != null && actionResult != actionResult as System.Web.Http.Results.NotFoundResult)
                {
                    var response = actionResult as OkNegotiatedContentResult<ScriptWorkflow>;
                    wfDisplay.workflowName = response.Content.WorkflowName;

                    nextNode = DetermineNextNode((ScriptWorkflow)response.Content, currentNode, direction);

                    if (nextNode != null)
                    {
                        if (!direction.Equals(MoveDirection.Current) && nextNode.nodeName != null && nextNode.nodeType.Equals(NodeType.Section))
                            SessionManager.StoreNavigation(HttpContext.Session, nextNode.nodeName, wfDisplay.workflowName.ToString());

                        if (nextNode.nodeActions != "")
                            FireActions(nextNode.nodeActions);

                        switch (nextNode.nodeType)
                        {
                            case NodeType.Section:
                                AddtoWFHistory(response.Content.ScriptWorkflowID, nextNode.NodeUniqueID, response.Content.WorkflowName, nextNode.nodeName);
                                break;
                            case NodeType.Workflow:
                                return RedirectToAction("Display", "Workflow", new { id = WFC.GetScriptWorkflowId(nextNode.nodeName) });
                            case NodeType.PreviousWorkflow:
                                return RedirectToAction("DisplayByDirection", "Workflow", new { id = WFC.GetScriptWorkflowId(nextNode.nodeName), currentNode = nextNode.DocUID, moveDirection = MoveDirection.Current });
                            case NodeType.SignPost:
                                MoveDirection direction2 = direction == MoveDirection.Start ? MoveDirection.Forward : direction;
                                WFNodeInfo holdNode = nextNode;
                                nextNode = DetermineNextNode((ScriptWorkflow)response.Content, nextNode.NodeUniqueID, direction2);

                                if (nextNode == null)
                                {
                                    direction = MoveDirection.Current;
                                    return Display(Convert.ToInt32(Request.Form["hdnWorkflowId"]));
                                }
                                else
                                {
                                    if (!direction.Equals(MoveDirection.Current) && nextNode.nodeName != null)
                                    {
                                        SessionManager.StoreNavigation(HttpContext.Session, nextNode.nodeName, wfDisplay.workflowName.ToString());
                                        AddtoWFHistory(response.Content.ScriptWorkflowID, nextNode.NodeUniqueID, response.Content.WorkflowName, nextNode.nodeName);
                                    }

                                    if (nextNode.nodeType.Equals(NodeType.Workflow))
                                        return RedirectToAction("Display", "Workflow", new { id = WFC.GetScriptWorkflowId(nextNode.nodeName) });
                                }
                                FireActions(nextNode.nodeActions);
                                if (nextNode.nodeType.Equals(NodeType.SignPost))
                                {
                                    nextNode = DetermineNextNode((ScriptWorkflow)response.Content, nextNode.NodeUniqueID, direction2);

                                    if (nextNode == null)
                                    {
                                        direction = MoveDirection.Current;
                                        return Display(Convert.ToInt32(Request.Form["hdnWorkflowId"]));
                                    }
                                    else
                                    {
                                        if (!direction.Equals(MoveDirection.Current) && nextNode.nodeName != null)
                                        {
                                            SessionManager.StoreNavigation(HttpContext.Session, nextNode.nodeName, wfDisplay.workflowName.ToString());
                                            AddtoWFHistory(response.Content.ScriptWorkflowID, nextNode.NodeUniqueID, response.Content.WorkflowName, nextNode.nodeName);
                                        }

                                        if (nextNode.nodeType.Equals(NodeType.Workflow))
                                            return RedirectToAction("Display", "Workflow", new { id = WFC.GetScriptWorkflowId(nextNode.nodeName) });
                                    }
                                    FireActions(nextNode.nodeActions);
                                }
                                break;
                        }

                        if (IsNextNode((ScriptWorkflow)response.Content, nextNode.NodeUniqueID))
                            wfDisplay.showNext = true;
                        else
                            wfDisplay.showNext = false;
                    }
                    else
                    {
                        direction = MoveDirection.Current;
                        return DisplayPartial(Convert.ToInt32(Request.Form["hdnWorkflowId"]));
                    }
                }
                else
                {
                    error = string.Format("Workflow id {0} not found.", id.ToString());
                    throw new Exception(error);
                }

                if (TempData["ScrollPosition"] != null)
                    ViewBag.ScrollPosition = TempData["ScrollPosition"];
                else
                    ViewBag.ScrollPosition = "0";

                if (SessionManager.GetProgramParameterByKey("BTNNEXT", Session) != null && !string.IsNullOrEmpty(SessionManager.GetProgramParameterByKey("BTNNEXT", Session)))
                    ViewBag.NextButtonLabel = SessionManager.GetProgramParameterByKey("BTNNEXT", Session);
                else
                    ViewBag.NextButtonLabel = "Next";

                ViewBag.IsLoad = true;
                ViewBag.Search = false;
                nextNode.DocUID = id;

                if (SessionManager.GetWorkflowHistory(HttpContext.Session).Length >= 2)
                    wfDisplay.showPrevious = true;
                else
                    wfDisplay.showPrevious = false;

                wfDisplay.nextNode = nextNode;

                if (IsPartialView)
                {
                    IsPartialView = false;
                    return PartialView(ConfigurationManager.AppSettings["workflowView"], wfDisplay);
                }
                else
                    return View(ConfigurationManager.AppSettings["workflowView"], wfDisplay);
            }
            catch (Exception ex)
            {
                WorkflowHelper.SaveProgress(HttpContext.Session);
                Data.Log log = ex.Save(HttpContext.Session, WorkflowHelper.BuildCallData(HttpContext.Session), ImpactLevel.High, error);

                wfDisplay.CurrentNodeId = currentNode.ToString();
                wfDisplay.ContactID = SessionManager.GetContactId(HttpContext.Session).ToString();
                wfDisplay.ExceptionMessage = ex.Message;
                wfDisplay.StackTrace = ex.StackTrace;

                if (log != null)
                {
                    wfDisplay.ClassName = log.ClassName;
                    wfDisplay.MethodName = log.MethodName;
                }

                if (!string.IsNullOrEmpty(wfDisplay.Layout))
                    return View("WorkFlowErrorView", wfDisplay);
                else
                    return View("Error", new HandleErrorInfo(ex, "WorkflowController", "Display"));
            }
        }

        public ActionResult DisplayPartial(int id)
        {
            WorkflowDisplay wfDisplay = null;
            WFNodeInfo nextNode = null;
            ScreenViewer.API.WorkflowController WFC = null;
            string error = string.Empty;

            try
            {
                wfDisplay = new WorkflowDisplay();
                WFC = new API.WorkflowController();

                if (!ScreenViewer.SignInHelper.ValidateSignIn())
                    return RedirectToAction("Login", "Account");

                wfDisplay.workflowID = id.ToString();
                wfDisplay.Layout = SessionManager.GetLayoutHTML(HttpContext.Session);
                wfDisplay.Notifications = SessionManager.GetNotificationText(HttpContext.Session);
                wfDisplay.menuHTML = SessionManager.GetMenuHTML(HttpContext.Session);
                wfDisplay.callNotes = SessionManager.GetContactNotes(HttpContext.Session);

                var actionResult = WFC.GetScriptWorkflow(id);

                if (actionResult != null && actionResult != actionResult as System.Web.Http.Results.NotFoundResult)
                {
                    var response = actionResult as OkNegotiatedContentResult<ScriptWorkflow>;
                    wfDisplay.workflowName = response.Content.WorkflowName;

                    nextNode = DetermineNextNode((ScriptWorkflow)response.Content, currentNode, direction);

                    if (nextNode != null)
                    {
                        if (!direction.Equals(MoveDirection.Current) && nextNode.nodeName != null && nextNode.nodeType.Equals(NodeType.Section))
                            SessionManager.StoreNavigation(HttpContext.Session, nextNode.nodeName, wfDisplay.workflowName.ToString());

                        if (nextNode.nodeActions != "")
                            FireActions(nextNode.nodeActions);

                        switch (nextNode.nodeType)
                        {
                            case NodeType.Section:
                                AddtoWFHistory(response.Content.ScriptWorkflowID, nextNode.NodeUniqueID, response.Content.WorkflowName, nextNode.nodeName);
                                break;
                            case NodeType.Workflow:
                                return RedirectToAction("DisplayPartial", "Workflow", new { id = WFC.GetScriptWorkflowId(nextNode.nodeName) });
                            case NodeType.PreviousWorkflow:
                                return RedirectToAction("DisplayByDirection", "Workflow", new { id = WFC.GetScriptWorkflowId(nextNode.nodeName), currentNode = nextNode.DocUID, moveDirection = MoveDirection.Current });
                            case NodeType.SignPost:
                                MoveDirection direction2 = direction == MoveDirection.Start ? MoveDirection.Forward : direction;
                                WFNodeInfo holdNode = nextNode;
                                nextNode = DetermineNextNode((ScriptWorkflow)response.Content, nextNode.NodeUniqueID, direction2);

                                if (nextNode == null)
                                {
                                    direction = MoveDirection.Current;
                                    return Display(Convert.ToInt32(Request.Form["hdnWorkflowId"]));
                                }
                                else
                                {
                                    if (!direction.Equals(MoveDirection.Current) && nextNode.nodeName != null)
                                    {
                                        SessionManager.StoreNavigation(HttpContext.Session, nextNode.nodeName, wfDisplay.workflowName.ToString());
                                        AddtoWFHistory(response.Content.ScriptWorkflowID, nextNode.NodeUniqueID, response.Content.WorkflowName, nextNode.nodeName);
                                    }

                                    if (nextNode.nodeType.Equals(NodeType.Workflow))
                                        return RedirectToAction("DisplayPartial", "Workflow", new { id = WFC.GetScriptWorkflowId(nextNode.nodeName) });
                                }

                                FireActions(nextNode.nodeActions);
                                if (nextNode.nodeType.Equals(NodeType.SignPost))
                                {
                                    nextNode = DetermineNextNode((ScriptWorkflow)response.Content, nextNode.NodeUniqueID, direction2);

                                    if (nextNode == null)
                                    {
                                        direction = MoveDirection.Current;
                                        return Display(Convert.ToInt32(Request.Form["hdnWorkflowId"]));
                                    }
                                    else
                                    {
                                        if (!direction.Equals(MoveDirection.Current) && nextNode.nodeName != null)
                                        {
                                            SessionManager.StoreNavigation(HttpContext.Session, nextNode.nodeName, wfDisplay.workflowName.ToString());
                                            AddtoWFHistory(response.Content.ScriptWorkflowID, nextNode.NodeUniqueID, response.Content.WorkflowName, nextNode.nodeName);
                                        }

                                        if (nextNode.nodeType.Equals(NodeType.Workflow))
                                            return RedirectToAction("DisplayPartial", "Workflow", new { id = WFC.GetScriptWorkflowId(nextNode.nodeName) });
                                    }
                                    FireActions(nextNode.nodeActions);
                                }
                                break;
                        }

                        if (IsNextNode((ScriptWorkflow)response.Content, nextNode.NodeUniqueID))
                            wfDisplay.showNext = true;
                        else
                            wfDisplay.showNext = false;
                    }
                    else
                    {

                        direction = MoveDirection.Current;
                        return DisplayPartial(Convert.ToInt32(Request.Form["hdnWorkflowId"]));
                    }
                }
                else
                {
                    error = string.Format("Workflow id {0} not found.", id.ToString());
                    throw new Exception(error);
                }

                
                if (TempData["ScrollPosition"] != null)
                    ViewBag.ScrollPosition = TempData["ScrollPosition"];
                else
                    ViewBag.ScrollPosition = "0";

                if (SessionManager.GetProgramParameterByKey("BTNNEXT", Session) != null && !string.IsNullOrEmpty(SessionManager.GetProgramParameterByKey("BTNNEXT", Session)))
                    ViewBag.NextButtonLabel = SessionManager.GetProgramParameterByKey("BTNNEXT", Session);
                else
                    ViewBag.NextButtonLabel = "Next";

                ViewBag.IsLoad = false;
                nextNode.DocUID = id;

                if (SessionManager.GetWorkflowHistory(HttpContext.Session).Length >= 2)
                    wfDisplay.showPrevious = true;
                else
                    wfDisplay.showPrevious = false;

                wfDisplay.nextNode = nextNode;

                return PartialView(ConfigurationManager.AppSettings["workflowView"], wfDisplay);

            }
            catch (Exception ex)
            {
                WorkflowHelper.SaveProgress(HttpContext.Session);
                Data.Log log = ex.Save(HttpContext.Session, WorkflowHelper.BuildCallData(HttpContext.Session), ImpactLevel.High, error);

                wfDisplay.CurrentNodeId = currentNode.ToString();
                wfDisplay.ContactID = SessionManager.GetContactId(HttpContext.Session).ToString();
                wfDisplay.ExceptionMessage = ex.Message;
                wfDisplay.StackTrace = ex.StackTrace;

                if (log != null)
                {
                    wfDisplay.ClassName = log.ClassName;
                    wfDisplay.MethodName = log.MethodName;
                }

                if (!string.IsNullOrEmpty(wfDisplay.Layout))
                    return View("WorkFlowErrorView", wfDisplay);
                else
                    return View("Error", new HandleErrorInfo(ex, "WorkflowController", "Display"));
            }
        }

        [System.Web.Mvc.ActionName("DisplayByDirection")]
        public ActionResult Display(int id, int currentNode, MoveDirection moveDirection)
        {
            WorkflowDisplay wfDisplay = null;
            WFNodeInfo nextNode = null;
            ScreenViewer.API.WorkflowController WFC = null;
            string error = string.Empty;

            try
            {
                wfDisplay = new WorkflowDisplay();
                WFC = new API.WorkflowController();

                if (!ScreenViewer.SignInHelper.ValidateSignIn())
                    return RedirectToAction("Login", "Account");

                wfDisplay.workflowID = id.ToString();
                wfDisplay.Layout = SessionManager.GetLayoutHTML(HttpContext.Session);
                wfDisplay.Notifications = SessionManager.GetNotificationText(HttpContext.Session);
                wfDisplay.menuHTML = SessionManager.GetMenuHTML(HttpContext.Session);
                wfDisplay.callNotes = SessionManager.GetContactNotes(HttpContext.Session);

                var actionResult = WFC.GetScriptWorkflow(id);

                if (actionResult != null && actionResult != actionResult as System.Web.Http.Results.NotFoundResult)
                {
                    var response = actionResult as OkNegotiatedContentResult<ScriptWorkflow>;
                    wfDisplay.workflowName = response.Content.WorkflowName;

                    nextNode = DetermineNextNode((ScriptWorkflow)response.Content, currentNode, moveDirection);

                    if (nextNode != null)
                    {
                        if (!direction.Equals(MoveDirection.Current) && nextNode.nodeName != null)
                            SessionManager.StoreNavigation(HttpContext.Session, nextNode.nodeName, wfDisplay.workflowName.ToString());

                        if (nextNode.nodeActions != "" && FireAway)
                        {
                            FireActions(nextNode.nodeActions);
                        }

                        switch (nextNode.nodeType)
                        {
                            case NodeType.Section:
                                AddtoWFHistory(response.Content.ScriptWorkflowID, nextNode.NodeUniqueID, response.Content.WorkflowName, nextNode.nodeName);
                                break;
                            case NodeType.Workflow:
                                return RedirectToAction("DisplayPartial", "Workflow", new { id = WFC.GetScriptWorkflowId(nextNode.nodeName) });
                            case NodeType.PreviousWorkflow:
                                return RedirectToAction("DisplayByDirection", "Workflow", new { id = WFC.GetScriptWorkflowId(nextNode.nodeName), moveDirection = MoveDirection.Current });
                            case NodeType.SignPost:

                                nextNode = DetermineNextNode((ScriptWorkflow)response.Content, currentNode, direction);

                                if (!direction.Equals(MoveDirection.Current) && nextNode.nodeName != null)
                                    SessionManager.StoreNavigation(HttpContext.Session, nextNode.nodeName, wfDisplay.workflowName.ToString());

                                if (nextNode.nodeActions != "" & FireAway)
                                {
                                    FireActions(nextNode.nodeActions);
                                }
                                break;
                        }

                        if (IsNextNode((ScriptWorkflow)response.Content, nextNode.NodeUniqueID))
                            wfDisplay.showNext = true;
                        else
                            wfDisplay.showNext = false;
                    }
                    else
                    {

                        direction = MoveDirection.Current;
                        return DisplayPartial(Convert.ToInt32(Request.Form["hdnWorkflowId"]));
                    }
                }
                else
                {
                    error = string.Format("Workflow id {0} not found.", id.ToString());
                    throw new Exception(error);
                }


                if (SessionManager.GetWorkflowHistory(HttpContext.Session).Length >= 2)
                    wfDisplay.showPrevious = true;
                else
                    wfDisplay.showPrevious = false;

                if (SessionManager.GetProgramParameterByKey("BTNNEXT", Session) != null && !string.IsNullOrEmpty(SessionManager.GetProgramParameterByKey("BTNNEXT", Session)))
                    ViewBag.NextButtonLabel = SessionManager.GetProgramParameterByKey("BTNNEXT", Session);
                else
                    ViewBag.NextButtonLabel = "Next";

                wfDisplay.nextNode = nextNode;
                ViewBag.Search = false;

                return PartialView(ConfigurationManager.AppSettings["workflowView"], wfDisplay);
            }
            catch (Exception ex)
            {
                WorkflowHelper.SaveProgress(HttpContext.Session);
                Data.Log log = ex.Save(HttpContext.Session, WorkflowHelper.BuildCallData(HttpContext.Session), ImpactLevel.High, error);

                wfDisplay.CurrentNodeId = currentNode.ToString();
                wfDisplay.ContactID = SessionManager.GetContactId(HttpContext.Session).ToString();
                wfDisplay.ExceptionMessage = ex.Message;
                wfDisplay.StackTrace = ex.StackTrace;

                if (log != null)
                {
                    wfDisplay.ClassName = log.ClassName;
                    wfDisplay.MethodName = log.MethodName;
                }

                if (!string.IsNullOrEmpty(wfDisplay.Layout))
                    return View("WorkFlowErrorView", wfDisplay);
                else
                    return View("Error", new HandleErrorInfo(ex, "WorkflowController", "Display"));
            }
        }

        [System.Web.Mvc.HttpPost]
        public ActionResult myForm(FormCollection collection, string ButtonType)
        {
            IsPartialView = true;
            ViewBag.Search = false;

            if (string.IsNullOrEmpty(SessionManager.GetLayoutHTML(HttpContext.Session)))
            {
                return JavaScript(string.Format("window.location = '/Account/login/?returnUrl={0}'", Url.Encode(Request.Form["hdnSiteURL"].Replace(System.Web.HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority), string.Empty))));
            }

            if (ButtonType == "Next" || ButtonType == SessionManager.GetProgramParameterByKey("BTNNEXT", Session))
            {
                FireAway = true;
                currentNode = Convert.ToInt32(Request.Form["hdnCurrentNodeId"]);
                direction = MoveDirection.Forward;
                RetrieveResponses(collection);
                return DisplayPartial(Convert.ToInt32(Request.Form["hdnWorkflowId"]));
            }

            if (ButtonType == "Previous")
            {
                currentNode = Convert.ToInt32(Request.Form["hdnCurrentNodeId"]);
                direction = MoveDirection.Back;
                return DisplayPartial(Convert.ToInt32(Request.Form["hdnWorkflowId"]));
            }

            if (ButtonType == "Current" || ButtonType == "Return")
            {
                FireAway = false;
                currentNode = Convert.ToInt32(Request.Form["hdnCurrentNodeId"]);
                if (ButtonType == "Current")
                {
                    RetrieveResponses(collection);
                }

                foreach (string key in Request.Form.Keys)
                {
                    if (key == "div_position" && !string.IsNullOrEmpty(collection[key]))
                    {
                        TempData["ScrollPosition"] = collection[key];
                    }
                }

                direction = MoveDirection.Current;

                if (!string.IsNullOrEmpty(collection["hdnSectionOnlyId"]) && ButtonType != "Return")
                {
                    ScreenViewer.API.SectionController SC = new API.SectionController();
                    ViewBag.SectionName = SC.ScriptSectionName(Convert.ToInt32(collection["hdnSectionOnlyId"]));
                    ViewBag.Section = true;
                    ViewBag.SectionId = collection["hdnSectionOnlyId"];
                    return Display(Convert.ToInt32(Request.Form["hdnWorkflowId"]));
                }
                else
                {
                    return DisplayPartial(Convert.ToInt32(Request.Form["hdnWorkflowId"]));
                }
            }

            if (ButtonType == "Back")
            {
                FireAway = false;
                currentNode = Convert.ToInt32(Request.Form["hdnCurrentNodeId"]);
                direction = MoveDirection.Current;
                return DisplayPartial(Convert.ToInt32(Request.Form["hdnWorkflowId"]));
            }

            if (ButtonType == "Schedule Callback")
            {
                DateTime callBack = DateTime.Parse(Request["datetimepicker"]);

                ContactTaskViewModel task = new ContactTaskViewModel();
                task.TaskType = "Callback";
                task.ContactId = SessionManager.GetContactId(Session);
                task.TaskColor = "#f8a398";
                task.Start = callBack;
                task.End = callBack.AddMinutes(30);
                task.Description = "Callback schedule at " + callBack.ToLongDateString() + "at " + callBack.ToShortTimeString();
                task.LeadID = SessionControl.SessionManager.GetProgramParameterByKey("LeadID", HttpContext.Session);
                task.CreatedDate = DateTime.Now;
                task.ModifiedDate = DateTime.Now;
                task.UserID = SessionControl.SessionManager.GetUserId(HttpContext.Session);
                task.ClientID = SessionControl.SessionManager.GetClientId(HttpContext.Session);

                SchedulerContactTaskService taskService = new SchedulerContactTaskService();
                taskService.Insert(task, ModelState);

                if (task.TaskID != 0)
                    ViewBag.callback = string.Format("Callback has been scheduled successfully at {0}", callBack);

                TempData["Expand"] = true;
                FireAway = false;
                currentNode = Convert.ToInt32(Request.Form["hdnCurrentNodeId"]);
                RetrieveResponses(collection);
                direction = MoveDirection.Current;
                return DisplayPartial(Convert.ToInt32(Request.Form["hdnWorkflowId"]));
            }

            if (ButtonType == "Switch Language")
            {
                if (!string.IsNullOrEmpty(SessionControl.SessionManager.ReturnParameter(HttpContext.Session, "SwitchLanguage")))
                {
                    SessionControl.SessionManager.StoreProgramParameter("SwitchLanguage", string.Empty, HttpContext.Session);
                }
                else
                {
                    SessionControl.SessionManager.StoreProgramParameter("SwitchLanguage", "Yes", HttpContext.Session);
                }

                FireAway = false;
                currentNode = Convert.ToInt32(Request.Form["hdnCurrentNodeId"]);
                RetrieveResponses(collection);

                direction = MoveDirection.Current;
                return DisplayPartial(Convert.ToInt32(Request.Form["hdnWorkflowId"]));
            }


            if (ButtonType == "Search")
            {

                FireAway = false;
                currentNode = Convert.ToInt32(Request.Form["hdnCurrentNodeId"]);
                RetrieveResponses(collection);

                direction = MoveDirection.Current;

                ViewBag.SearchTerm = Request.Form["SearchTerm"];
                ViewBag.Search = true;
                return DisplayPartial(Convert.ToInt32(Request.Form["hdnWorkflowId"]));

            }

            if (ButtonType == "Section")
            {
                FireAway = false;
                currentNode = Convert.ToInt32(Request.Form["hdnCurrentNodeId"]);
                RetrieveResponses(collection);
                direction = MoveDirection.Current;

                string sectionId = Request["hdnSectionId"];

                if (!string.IsNullOrEmpty(sectionId))
                {
                    ScreenViewer.API.SectionController SC = new API.SectionController();
                    ViewBag.SectionName = SC.ScriptSectionName(Convert.ToInt32(sectionId));
                    ViewBag.Section = true;
                    ViewBag.SectionId = sectionId;
                }
                return Display(Convert.ToInt32(Request.Form["hdnWorkflowId"]));
            }

            if (ButtonType.StartsWith("Action"))
            {
                string[] action = ButtonType.Split('_');
                FireActions(action[1]);

                FireAway = false;
                currentNode = Convert.ToInt32(Request.Form["hdnCurrentNodeId"]);

                if (ButtonType == "Current")
                {
                    RetrieveResponses(collection);
                }

                direction = MoveDirection.Current;
                return DisplayPartial(Convert.ToInt32(Request.Form["hdnWorkflowId"]));
            }

            return JavaScript("Button Not Found!");
        }



        //[System.Web.Mvc.HttpPost]
        //[MultipleButton(Name = "action", Argument = "Action")]
        //public ActionResult Action(FormCollection collection, List<ScriptSectionLayout> SectionLayout)
        //{
        //    currentNode = Convert.ToInt32(Request.Form["hdnCurrentNodeId"]);
        //    direction = MoveDirection.Current;

        //    RetrieveResponses(collection);

        //    FireActions(collection["action:Action"]);

        //    FireAway = false;

        //    return Display(Convert.ToInt32(Request.Form["hdnWorkflowId"]));
        //}

        public void RetrieveResponses(FormCollection collection)
        {

            List<QuestVal> questvals = new List<QuestVal>();
            List<ItemCounts> icounts = new List<ItemCounts>();
            List<string> shownItems = new List<string>();
            List<string> setKeyIDs = new List<string>();
            List<string> QuestionsShown;
            //Request.Form
            try
            {
                QuestionsShown = Regex.Split(Request.Form["SectionLayout"].ToString(), ",").ToList();
            }
            catch
            {
                return;
            }
                string setKey = "";

            foreach (string key in Request.Form.Keys)
            {
                if (key.StartsWith("SPQuestion_") && !key.Contains("_ddl") && !key.Contains("_mddl"))
                {
                    QuestVal QV = new QuestVal();
                    var value = collection[key];
                    string Qindex = Regex.Split(key, "_")[1];
                    QV.QuestionID = Qindex;
                    QV.Response = (string)value;

                    if (Request.Form.AllKeys.Contains(key + "_ddl"))
                    {
                        QV.DisplayResponse = collection[key + "_ddl"];
                    }

                    if (Request.Form.AllKeys.Contains(key + "_mddl"))
                    {
                        QV.Response = QV.Response.Replace(",", "~");
                        QV.DisplayResponse = collection[key + "_mddl"];
                    }

                    questvals.Add(QV);

                    int listloc = 0;

                    foreach (string qid in QuestionsShown)
                    {
                        if (qid == QV.QuestionID)
                        {
                            QuestionsShown.RemoveAt(listloc);
                            break;
                        }
                        listloc++;
                    }


                }

                if (key.StartsWith("SPsetKey_"))
                {
                    string val = collection[key];
                    string cid = key.Remove(0, 9);
                    setKeyIDs.Add(cid);

                }

                if (key.StartsWith("SPShownItemCollection_"))
                {
                    string collectItems = collection[key];

                    shownItems.AddRange(Regex.Split(collectItems, "~~").ToList());

                }
                if (key.StartsWith("SPselectQuestionValues_"))
                {

                    string thevalues = collection[key];

                    if (thevalues.StartsWith("~~"))
                    {
                        thevalues = thevalues.Remove(0, 2);
                    }
                    List<string> selectedVals = Regex.Split(thevalues, "~~").ToList();



                }

                if (key.StartsWith("SPselectObjectValue_"))
                {


                    QuestVal QV = new QuestVal();
                    var value = collection[key];
                    string Qindex = Regex.Split(key, "_")[1];
                    QV.QuestionID = Qindex;
                    QV.Response = (string)value;
                    QV.DisplayResponse = (string)value;
                    questvals.Add(QV);

                    int listloc = 0;

                    foreach (string qid in QuestionsShown)
                    {
                        if (qid == QV.QuestionID)
                        {
                            QuestionsShown.RemoveAt(listloc);
                            break;
                        }
                        listloc++;
                    }

                }


                if (key.StartsWith("SPShownItemCollection_"))
                {
                    string collectItems = collection[key];

                    shownItems.AddRange(Regex.Split(collectItems, "~~").ToList());

                }

                


                if (key.StartsWith("SPselectItemValues"))
                {
                    string thevalues = collection[key];
                    string itemSelector = Regex.Split(key, "_")[2];
                    string ind = Regex.Split(key, "_")[1];
                    if (thevalues != "~NoActivity~")
                    {
                        if (thevalues.StartsWith("~~"))
                        {
                            thevalues = thevalues.Remove(0, 2);
                        }
                        List<string> selectedVals = Regex.Split(thevalues, "~~").ToList();
                        foreach (string item in selectedVals)
                        {
                            ItemCounts IC = new ItemCounts();
                            IC.itemCode = item;
                            IC.itemQuantity = 1;
                            IC.ownerOIS = itemSelector;
                            icounts.Add(IC);
                        }
                        string collectItemsx = collection["SPSelectItemCollection_" + ind + "_" + itemSelector];

                        shownItems.AddRange(Regex.Split(collectItemsx, "~~").ToList());

                    }


                }
                if (key.StartsWith("SPitemcheck_"))
                {
                    string itemSelector = Regex.Split(key, "_")[2];
                    string itemCode = key.Remove(0, 12 + itemSelector.Length + 3);
                    string retval = collection[key];
                    Int32 quant = (retval == "on") ? 1 : 0;
                    ItemCounts IC = new ItemCounts();
                    IC.itemCode = itemCode;
                    IC.itemQuantity = quant;
                    IC.ownerOIS = itemSelector;
                    icounts.Add(IC);

                }
                if (key.StartsWith("SPitemquant_"))
                {
                    string itemSelector = Regex.Split(key, "_")[2];
                    string itemCode = key.Remove(0, 12 + itemSelector.Length + 3);
                    Int32 quant = Convert.ToInt32(collection[key]);
                    ItemCounts IC = new ItemCounts();
                    IC.itemCode = itemCode;
                    IC.itemQuantity = quant;
                    IC.ownerOIS = itemSelector;
                    icounts.Add(IC);
                }

                if (key == "SP_Notes")
                {
                    collection[key] = collection[key].Replace("\r", "");
                    collection[key] = collection[key].Replace("\n", "");
                    SessionManager.StoreContactNotes(HttpContext.Session, collection[key]);
                }
            }

            foreach (string qid in QuestionsShown)
            {
                QuestVal QV = new QuestVal();
                QV.QuestionID = qid;
                QV.Response = "";
                questvals.Add(QV);
            }

            foreach (string item in shownItems)
            {

                bool foundit = false;
                foreach (ItemCounts iC in icounts)
                {
                    if (item == iC.itemCode)
                    {
                        foundit = true;
                        break;

                    }
                }
                if (!foundit)
                {
                    ItemCounts IC = new ItemCounts();
                    IC.itemCode = item;
                    IC.itemQuantity = 0;
                    icounts.Add(IC);

                }
            }

            List<ItemOrdered> iOrd = new List<ItemOrdered>();

            foreach (ItemCounts iC in icounts)
            {
                ItemOrdered io = new ItemOrdered();
                io.ItemCode = iC.itemCode;
                io.ItemQuantity = iC.itemQuantity;
                io.setKey = (setKeyIDs.Exists(item => item == iC.ownerOIS)) ? true : false;
                io.oiOwner = Convert.ToInt32(iC.ownerOIS);
                iOrd.Add(io);
            }


            SessionManager.AddUpdateOrderedItems(iOrd, HttpContext.Session);

            if (questvals.Count > 0)
            {
                SessionManager.AddUpdateQuestions(questvals, HttpContext.Session);

            }
        }

        public ActionResult DisplayText(decimal id)
        {
            ScreenViewer.API.Elements.TextHTMLController THC = new API.Elements.TextHTMLController();
            var actionResult = THC.RenderScriptTextHTML(id);
            var response = actionResult as OkNegotiatedContentResult<string>;
            var renderedstring = response.Content;
            ViewBag.RenderedSection = renderedstring;
            return View();
        }

        private WFNodeInfo DetermineNextNode(ScriptWorkflow scriptWorkflow, int currentNode, MoveDirection direction)
        {
            // Local Variables
            XmlSerializer xmlSerializer = null;
            StringReader stringReader = null;
            WorkflowNodes workflowNodes = null;
            WFNodeInfo nextNode = null;
            //wfHistory[] WorkflowHistory = null;
            ArrayList WFHistArray = null;

            try
            {
                // Instantiate local variables
                stringReader = new StringReader(scriptWorkflow.WorkflowXML);
                WFHistArray = new ArrayList();

                // Deserialize the WorkFlow Nodes
                xmlSerializer = new XmlSerializer(typeof(WorkflowNodes));
                workflowNodes = (WorkflowNodes)xmlSerializer.Deserialize(stringReader);

                // Determin by direction
                switch (direction)
                {
                    case MoveDirection.Forward:
                        foreach (WFNodeInfo WFN in workflowNodes.Nodes)
                        {
                            if (WFN.NodeUniqueID == currentNode) //WorkflowObject.CurrentNode
                            {
                                Int32 ProcNode = DetermineNextSection(WFN);
                                foreach (WFNodeInfo WFN2 in workflowNodes.Nodes)
                                {
                                    if (WFN2.NodeUniqueID == ProcNode)
                                    {
                                        nextNode = WFN2;
                                        break;
                                    }
                                }
                                break;
                            }
                        }
                        break;

                    case MoveDirection.Back:

                        if (SessionManager.GetWorkflowHistory(HttpContext.Session) != null)
                        {
                            foreach (wfHistory wfhis in SessionManager.GetWorkflowHistory(HttpContext.Session))
                            {
                                WFHistArray.Add(wfhis);
                            }
                            if (WFHistArray.Count < 2)
                            {
                                return null;
                            }
                            wfHistory lastone = (wfHistory)WFHistArray[WFHistArray.Count - 2];

                            if (lastone.WorkFlowID == scriptWorkflow.ScriptWorkflowID && lastone.NodeID == currentNode)
                            {
                                return null;
                            }

                            if (lastone.WorkFlowID == scriptWorkflow.ScriptWorkflowID)
                            {
                                currentNode = lastone.NodeID;

                                WFHistArray.RemoveAt(WFHistArray.Count - 1);
                                WFHistArray.RemoveAt(WFHistArray.Count - 1);
                                SessionManager.StoreWorkflowHistory(HttpContext.Session, (wfHistory[])WFHistArray.ToArray(typeof(wfHistory)));
                                nextNode = DetermineNextNode(scriptWorkflow, currentNode, MoveDirection.Current);
                            }
                            else
                            {
                                nextNode = new WFNodeInfo();
                                nextNode.NodeUniqueID = lastone.WorkFlowID;
                                nextNode.nodeName = lastone.WorkFlowName;
                                nextNode.nodeType = NodeType.PreviousWorkflow;
                                nextNode.DocUID = lastone.NodeID; // Use DocUID parameter as  NodeId
                                WFHistArray.RemoveAt(WFHistArray.Count - 1);
                                WFHistArray.RemoveAt(WFHistArray.Count - 1);
                                SessionManager.StoreWorkflowHistory(HttpContext.Session, (wfHistory[])WFHistArray.ToArray(typeof(wfHistory)));
                                return nextNode;
                            }
                        }
                        else
                        {
                            return null;
                        }
                        break;

                    case MoveDirection.Current:
                        foreach (WFNodeInfo WFN in workflowNodes.Nodes)
                        {
                            if (WFN.NodeUniqueID == currentNode)
                            {
                                nextNode = WFN;
                                break;
                            }
                        }
                        break;

                    case MoveDirection.Start:
                        foreach (WFNodeInfo wFNodeInfo in workflowNodes.Nodes)
                        {
                            Int32 ProcNode = DetermineNextSection(wFNodeInfo);
                            foreach (WFNodeInfo wFNodeInfo2 in workflowNodes.Nodes)
                            {
                                if (wFNodeInfo2.NodeUniqueID == ProcNode)
                                {
                                    nextNode = wFNodeInfo2;
                                    break;
                                }
                            }
                            break;
                        }
                        break;

                    default:
                        break;

                }

                if (nextNode == null) //&& nextNode.nodeType.Equals(NodeType.SignPost)
                {
                    nextNode = DetermineNextNode(scriptWorkflow, nextNode.NodeUniqueID, MoveDirection.Forward);
                }

                return nextNode;
            }
            catch
            {
                return null;
            }
            finally
            {
                xmlSerializer = null;
                stringReader = null;
                workflowNodes = null;
                WFHistArray = null;
            }
        }

        private bool IsNextNode(ScriptWorkflow scriptWorkflow, int currentNode)
        {
            // Local Variables
            XmlSerializer xmlSerializer = null;
            StringReader stringReader = null;
            WorkflowNodes workflowNodes = null;
            WFNodeInfo nextNode = null;

            try
            {
                // Instantiate local variables
                stringReader = new StringReader(scriptWorkflow.WorkflowXML);
                WFHistArray = new ArrayList();

                // Deserialize the WorkFlow Nodes
                xmlSerializer = new XmlSerializer(typeof(WorkflowNodes));
                workflowNodes = (WorkflowNodes)xmlSerializer.Deserialize(stringReader);

                foreach (WFNodeInfo WFN in workflowNodes.Nodes)
                {
                    if (WFN.NodeUniqueID == currentNode)
                    {
                        Int32 ProcNode = DetermineNextSection(WFN);
                        foreach (WFNodeInfo WFN2 in workflowNodes.Nodes)
                        {
                            if ((WFN2.NodeUniqueID == ProcNode) || (WFN.Conditions != null && WFN.Conditions.Count() > 0))
                            {
                                nextNode = WFN2;
                                break;
                            }
                        }
                        break;
                    }
                }

                return (nextNode != null);
            }
            catch
            {
                return false;
            }
            finally
            {
                xmlSerializer = null;
                stringReader = null;
                workflowNodes = null;
            }
        }

        private void AddtoWFHistory(Int32 WFID, Int32 NodeID, string wfname, string sectionname)
        {
            if (WFHistArray == null)
            {
                WFHistArray = new ArrayList();
            }

            if (SessionManager.GetWorkflowHistory(HttpContext.Session) != null)
            {
                WFHistArray.Clear();
                foreach (wfHistory wf in SessionManager.GetWorkflowHistory(HttpContext.Session))
                {
                    if (wf.WorkFlowID > 0)
                    {
                        WFHistArray.Add(wf);
                    }
                }
            }

            wfHistory wfh = new wfHistory();
            wfh.WorkFlowID = WFID;
            wfh.NodeID = NodeID;
            wfh.WorkFlowName = wfname;
            wfh.SectionName = sectionname;

            if (WFHistArray.Count > 0)
            {
                wfHistory whist = (wfHistory)WFHistArray[WFHistArray.Count - 1];
                if (wfh.NodeID == whist.NodeID && wfh.WorkFlowID == whist.WorkFlowID)
                {
                    return;
                }

            }
            WFHistArray.Add(wfh);
            SessionManager.StoreWorkflowHistory(HttpContext.Session, (wfHistory[])WFHistArray.ToArray(typeof(wfHistory)));
        }

        private void FireActions(string actionlist)
        {
            if (actionlist != null && actionlist != "" && actionlist != "::")
            {

                string[] actions = Regex.Split(actionlist, ",");

                if (actions.Length > 1)
                {
                    //Array.Sort(actions, StringComparer.InvariantCulture);
                    Array.Sort(actions, (a, b) => string.Compare(a.Substring(6), b.Substring(6)));
                }

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
                            //case "APIPOST":
                            //    WorkflowHelper.APIPOST(HttpContext.Session, scriptAction.ActionXML);
                            //    break;
                            case "APIPOST":
                                WorkflowHelper.APIPOSTCOMPLEX(HttpContext.Session, scriptAction.ActionXML);
                                break;
                            case "APIPUT":
                                WorkflowHelper.APIPUT(HttpContext.Session, scriptAction.ActionXML);
                                break;
                            case "APIGETPLAIN":
                                WorkflowHelper.APIGETPLAIN(HttpContext.Session, scriptAction.ActionXML);
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
                            case "Link":
                                PopUps.Add(scriptAction.ActionXML);
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
                            //case "Update Data Object Field":
                            //    WorkflowHelper.UpdateDataObjectField(HttpContext.Session, scriptAction.ActionXML);
                            //    break;
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

        public Int32 DetermineNextSection(WFNodeInfo CurrentNode)
        {
            Int32 gotonode = -1;

            if (CurrentNode.Conditions == null)
            {
                return gotonode;
            }

            foreach (WFCondition Cond in CurrentNode.Conditions)
            {
                if (Cond.conditionType == ConditionType.Default)
                {
                    gotonode = Cond.linktoNode;
                }
                else
                {
                    switch (Cond.conditionType)
                    {
                        case ConditionType.Question:
                            try
                            {
                                string qresp = GetQuestionResponse(Cond.Question.ToString());
                                if (qresp == Cond.QuestionResponse)
                                {
                                    return Cond.linktoNode;
                                }
                            }
                            catch
                            {
                            }
                            break;

                        case ConditionType.DataObject:
                            try
                            {

                                string doValue = GetDataObjectValue(Cond.DataObject);

                                if (doValue == Cond.DataObjectValue)
                                {
                                    return Cond.linktoNode;
                                }

                            }
                            catch
                            {

                            }
                            break;
                        case ConditionType.Clause:
                            ClauseEvaluator CE = new ClauseEvaluator(HttpContext.Session);
                            bool usesection = CE.EvaluateClause(Cond.ClauseID.ToString(), Cond.Result);

                            if (usesection)
                            {
                                return Cond.linktoNode;
                            }

                            break;
                        case ConditionType.LinkTo:
                            return Cond.linktoNode;

                    }
                }
            }
            return gotonode;

        }

        public string GetQuestionResponse(String QuestionID)
        {
            if (!string.IsNullOrEmpty(SessionManager.GetQuestionResponse(QuestionID, HttpContext.Session)))
            {
                return SessionManager.GetQuestionResponse(QuestionID, HttpContext.Session);
            }
            else
            {
                return string.Empty;
            }
        }
        public string GetDataObjectValue(String DataObject)
        {

            object x = DataObjects.ReturnValue(DataObject, HttpContext.Session);
            return x.ToString();


        }
    }

    public class FormData
    {
        public string SectionName { get; set; }
        public string QuestionId { get; set; }
        public string Response { get; set; }
    }

    static class Extensions
    {
        /// <summary>
        /// Convert ArrayList to List.
        /// </summary>
        public static List<T> ToList<T>(this ArrayList arrayList)
        {
            List<T> list = new List<T>(arrayList.Count);
            foreach (T instance in arrayList)
            {
                list.Add(instance);
            }
            return list;
        }
    }
}