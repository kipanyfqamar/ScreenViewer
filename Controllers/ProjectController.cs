using System;
using System.IO;
using System.Data;
using System.Configuration;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.Mvc.Html;
using System.Web.Http.Results;
using System.Web.Script.Serialization;
using ScreenViewer.Models;
using ScreenViewer.Data;
using ScreenViewer.SessionControl;
using ScreenViewer.API.Elements;
using Microsoft.AspNet.Identity;
using Kendo.Mvc;
using Kendo.Mvc.UI;
using ScreenViewer.Authentication;

namespace ScreenViewer.Controllers
{
    public class ProjectController : Controller
    {
        public bool addparams = false;
        public string url = string.Empty;
        public DataTable menuTable = null;

        [ActionName("DisplayProject")]
        public ActionResult DisplayByName(string projectname)
        {
            try
            {
                Session["ReturnURL"] =  System.Web.HttpContext.Current.Request.Url.PathAndQuery;

                if (!ScreenViewer.SignInHelper.ValidateSignIn())
                    return RedirectToAction("Login", "Account", new { ReturnUrl = Session["ReturnURL"].ToString() });

                ScreenViewer.API.ProjectController PC = new API.ProjectController();
                int projectId = PC.GetProjectId(projectname);
                SessionManager.ClearSessionData(HttpContext.Session);
                AddProgramParameter();
                TempData["addparams"] = true;
                return RedirectToAction("Display", "Project", new { id = projectId });
            }
            catch (Exception ex)
            {
                Data.Log log = ex.Save(HttpContext.Session, WorkflowHelper.BuildCallData(HttpContext.Session), ImpactLevel.High);
                ViewBag.URL = System.Web.HttpContext.Current.Request.Url;
                return View("Error", new HandleErrorInfo(ex, "ProjectController", "DisplayByName"));
            }
        }

        public ActionResult Display(int id)
        {
            try
            {
                ScreenViewer.API.ProjectController PC = new API.ProjectController();
                var actionResult = PC.GetScriptProject(id);

                var response = actionResult as OkNegotiatedContentResult<ScriptProject>;
                ScriptProject scriptProject = response.Content;

                if (TempData["addparams"] == null || !(bool)TempData["addparams"])
                {
                    SessionManager.ClearSessionData(HttpContext.Session);
                    if (!ScreenViewer.SignInHelper.ValidateSignIn()) return RedirectToAction("Login", "Account", new { ReturnUrl = System.Web.HttpContext.Current.Request.Url.ToString() });
                    AddProgramParameter();
                }

                SessionControl.SessionManager.StoreProjectId(HttpContext.Session, id);
                SessionControl.SessionManager.StoreProjectName(HttpContext.Session, scriptProject.ProjectName);
                SessionControl.SessionManager.StoreProjectDescription(HttpContext.Session, scriptProject.ProjectDesc);
                SessionControl.SessionManager.StoreScreenLayout(HttpContext.Session, scriptProject.ScreenLayout);
                SessionControl.SessionManager.StoreNotificationText(HttpContext.Session, scriptProject.NotificationText);

                if (!string.IsNullOrEmpty(scriptProject.ScreenLayout))
                {
                    ScreenViewer.API.WorkflowLayoutsController WLC = new ScreenViewer.API.WorkflowLayoutsController();
                    var actionResult2 = WLC.GetWorkflowLayoutString(SessionManager.GetScreenLayout(HttpContext.Session), SessionManager.GetClientId(HttpContext.Session));

                    if (actionResult2 != null && actionResult2 != actionResult2 as System.Web.Http.Results.NotFoundResult)
                    {
                        var layoutResponse = actionResult2 as OkNegotiatedContentResult<string>;
                        SessionControl.SessionManager.StoreLayoutHTML(HttpContext.Session, layoutResponse.Content);
                    }
                }

                if (scriptProject.ScriptProjectWorkflows.Count > 0)
                {
                    if (scriptProject.RequireKey.Value)
                    {
                        string key = SessionControl.SessionManager.GetScriptParameterByKey("key", HttpContext.Session);

                        if (!string.IsNullOrEmpty(key))
                        {
                            if (!key.Equals(scriptProject.KeyText.ToString()))
                            {
                                throw new Exception(string.Format("Unable to verify the project key {0}.", key));
                            }
                        }
                        else
                        {
                            throw new Exception(string.Format("Project key is missing."));
                        }
                    }

                    // Create the contact record
                    CreateContactRecord(scriptProject.ProjectName);

                    if (scriptProject.LogoImageID != null && scriptProject.LogoImageID != 0)
                    {
                        var imageResult = PC.GetScriptProjectImage(scriptProject.LogoImageID.Value);
                        var imageResponse = imageResult as OkNegotiatedContentResult<ScriptImage>;
                        SessionControl.SessionManager.StoreLogoImage(HttpContext.Session, ((ScriptImage)imageResponse.Content).Image);
                    }

                    if (scriptProject.ScriptMenuID != null && scriptProject.ScriptMenuID != 0)
                    {
                        var scriptMenu = PC.GetScriptProjectMenu(scriptProject.ScriptMenuID.Value);
                        var menuResponse = scriptMenu as OkNegotiatedContentResult<ScriptMenu>;

                        ScriptUL ul;
                        using (StringReader sr = new StringReader(menuResponse.Content.MenuXML))
                        {
                            XmlSerializer xs = new XmlSerializer(typeof(ScriptUL));
                            ul = (ScriptUL)xs.Deserialize(sr);
                        }

                        StringWriter stringWriter = new StringWriter();
                        string menuType = "Navbar";
                        string addClass = "";
                        using (HtmlTextWriter writer = new HtmlTextWriter(stringWriter))
                        {
                            switch (menuType)
                            {
                                case "Tabbed":
                                    addClass = "nav nav-tabs";
                                    break;
                                case "Pills":
                                    addClass = "nav nav-pills";
                                    break;
                                case "Navbar":
                                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "navbar navbar-default");
                                    writer.RenderBeginTag("nav");

                                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "container");
                                    writer.RenderBeginTag(HtmlTextWriterTag.Div);

                                    addClass = "nav navbar-nav";
                                    break;
                            }

                            writer.AddAttribute(HtmlTextWriterAttribute.Class, addClass);
                            writer.RenderBeginTag(HtmlTextWriterTag.Ul); // Begin #1

                            foreach (ScriptLI li in ul.LIArray)
                            {
                                writer.Write(PopulateLItem(li));
                            }

                            writer.RenderEndTag();

                            if (menuType == "Navbar")
                            {
                                writer.RenderEndTag();
                            }
                        }

                        SessionControl.SessionManager.StoreMenu(HttpContext.Session, stringWriter.ToString());
                    }

                    if (scriptProject.ShowWorkflowSection.Value)
                    {
                        SessionControl.SessionManager.StoreProgramParameter("Section", "Yes", HttpContext.Session);
                    }

                    if (scriptProject.ShowWorkflowSection.Value)
                    {
                        SessionControl.SessionManager.StoreProgramParameter("Workflow", "Yes", HttpContext.Session);
                    }

                    if (scriptProject.ShowNotification.Value)
                    {
                        SessionControl.SessionManager.StoreProgramParameter("Notification", "Yes", HttpContext.Session);
                    }

                    if (scriptProject.ShowNotes.Value)
                    {
                        SessionControl.SessionManager.StoreProgramParameter("CallNotes", "Yes", HttpContext.Session);
                    }

                    if (scriptProject.ShowLead.Value)
                    {
                        SessionControl.SessionManager.StoreProgramParameter("LeadInformation", "Yes", HttpContext.Session);
                    }

                    if (scriptProject.ShowCallback.Value)
                    {
                        SessionControl.SessionManager.StoreProgramParameter("ScheduleCallback", "Yes", HttpContext.Session);
                    }

                    if (scriptProject.ShowLanguage != null && scriptProject.ShowLanguage.Value)
                    {
                        SessionControl.SessionManager.StoreProgramParameter("ShowLanguage", "Yes", HttpContext.Session);
                    }

                    ScriptProjectWorkflow workFlow = scriptProject.ScriptProjectWorkflows.Where(c => c.ScriptWorkflowActiveDate <= DateTime.Now).OrderByDescending(t => t.ScriptWorkflowActiveDate).FirstOrDefault();
                    return RedirectToAction("Display", "Workflow", new { id = workFlow.ScriptWorkflowID });
                }
                else
                {
                    return View("Display");
                }
            }
            catch (Exception ex)
            {
                Data.Log log = ex.Save(HttpContext.Session, WorkflowHelper.BuildCallData(HttpContext.Session), ImpactLevel.High);
                ViewBag.URL = SessionManager.GetScriptURL(Session);
                return View("Error", new HandleErrorInfo(ex, "ProjectController", "DisplayByName"));
            }
        }

        private bool RetrieveNewestContact()
        {
            bool result = false;

            try
            {
                if (!string.IsNullOrEmpty(SessionManager.GetProgramParameterByKey("LeadRecordId", HttpContext.Session)))
                {
                    result = true;
                    string leadId = SessionManager.GetProgramParameterByKey("LeadRecordId", HttpContext.Session);
                    ScreenViewer.API.ExternalData.ContactRecordController contactController = new API.ExternalData.ContactRecordController();

                    if (leadId != "" && leadId != null)
                    {
                        var actionResult = contactController.GetContactRecordByLeadId(leadId);
                        var response = actionResult as OkNegotiatedContentResult<ContactRecord>;

                        if (response != null)
                        {
                            ContactRecord contactRecord = response.Content;

                            if (contactRecord.ContactRecordDetails.Count > 0)
                            {
                                List<QuestVal> questvals = new List<QuestVal>();

                                foreach (var item in contactRecord.ContactRecordDetails)
                                {
                                    QuestVal QV = new QuestVal();
                                    QV.QuestionID = item.QuestionId.ToString();
                                    QV.Response = item.QuestionResponseValue;
                                    QV.DisplayResponse = item.QuestionResponseText;
                                    questvals.Add(QV);
                                }

                                SessionManager.AddUpdateQuestions(questvals, HttpContext.Session);
                            }
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private void CreateContactRecord(string projectName)
        {
            System.Web.Http.IHttpActionResult result = null;
            List<ContactAttribute> lstContactAttribute = null;
            ContactAttribute contactAttribute = null;
            ContactRecord contactRecord = null;
            Dictionary<string, string> scriptsParameter;
            DateTime timeUtc = DateTime.UtcNow;
            string previous = string.Empty;
            bool isLeadClientFound = false;

            try
            {
                contactRecord = new ContactRecord();
                lstContactAttribute = new List<ContactAttribute>();
                scriptsParameter = SessionManager.GetScriptParameter(HttpContext.Session);

                if (!string.IsNullOrEmpty(SessionManager.GetProgramParameterByKey("CallID", HttpContext.Session)))
                    contactRecord.ClientCallId = SessionManager.GetProgramParameterByKey("CallID", HttpContext.Session);
                else
                    contactRecord.ClientCallId = "N/P";

                if (!string.IsNullOrEmpty(SessionManager.GetProgramParameterByKey("ANI", HttpContext.Session)))
                    contactRecord.ANI = SessionManager.GetProgramParameterByKey("ANI", HttpContext.Session);
                else
                    contactRecord.ANI = "N/P";

                if (!string.IsNullOrEmpty(SessionManager.GetProgramParameterByKey("Previous", HttpContext.Session)))
                    previous = SessionManager.GetProgramParameterByKey("Previous", HttpContext.Session);

                if (string.IsNullOrEmpty(SessionManager.GetUserId(HttpContext.Session)) && string.IsNullOrEmpty(SessionManager.GetClientId(HttpContext.Session)))
                    throw new Exception("User OR Client not found while creating contact record.");

                try
                {
                    TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(ConfigurationManager.AppSettings["timeZone"]);
                    DateTime dateTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, timeZone);
                    contactRecord.CallStartDateTime = dateTime;
                    contactRecord.ModifiedDate = dateTime;
                    contactRecord.ProjectName = projectName;
                    contactRecord.CallState = "INPRC";
                    contactRecord.UserId = SessionManager.GetUserId(HttpContext.Session);
                    contactRecord.ClientId = SessionManager.GetClientId(HttpContext.Session);

                    if (!string.IsNullOrEmpty(SessionManager.ReturnParameter(HttpContext.Session, "LeadID")))
                        contactRecord.LeadRecordId = SessionManager.ReturnParameter(HttpContext.Session, "LeadID");

                    if (!string.IsNullOrEmpty(SessionManager.GetScriptURL(Session)))
                        contactRecord.URL = SessionManager.GetScriptURL(Session);

                    foreach (var item in scriptsParameter)
                    {
                        contactAttribute = new ContactAttribute();
                        contactAttribute.ContactAttributeName = item.Key;
                        contactAttribute.ContactAttributeValue = item.Value;
                        contactAttribute.CreatedDate = contactRecord.CallStartDateTime.Value;
                        contactAttribute.ClientId = SessionManager.GetClientId(HttpContext.Session);
                        lstContactAttribute.Add(contactAttribute);
                    }

                    contactRecord.ContactAttributes = lstContactAttribute;

                }
                catch { }

                ScreenViewer.API.ExternalData.ContactRecordController CRC = new API.ExternalData.ContactRecordController();

                // Check LeadRecord and ClientCallId Exist then retrive client call id
                if (!string.IsNullOrEmpty(contactRecord.LeadRecordId) && !string.IsNullOrEmpty(contactRecord.ClientCallId))
                {
                    result = CRC.GetContactRecordByLeadClient(contactRecord.LeadRecordId, contactRecord.ClientCallId);

                    if (result != null && result != result as System.Web.Http.Results.NotFoundResult)
                    {
                        isLeadClientFound = true;
                        var response = result as OkNegotiatedContentResult<ContactRecord>;
                        contactRecord = response.Content;
                        SessionControl.SessionManager.StoreContactId(HttpContext.Session, contactRecord.ContactId);
                        SessionManager.StoreProgramParameter("ClientCallId", contactRecord.ClientCallId, Session);
                    }
                }

                if (!isLeadClientFound)
                {

                    if (string.IsNullOrEmpty(previous))
                        result = CRC.PostContactRecord(contactRecord);
                    else
                        result = CRC.GetContactRecordByContactId(Convert.ToInt32(previous));

                    if (result != null && result != result as System.Web.Http.Results.NotFoundResult)
                    {
                        var response = result as OkNegotiatedContentResult<ContactRecord>;
                        contactRecord = response.Content;
                        SessionControl.SessionManager.StoreContactId(HttpContext.Session, contactRecord.ContactId);
                        SessionManager.StoreProgramParameter("ClientCallId", contactRecord.ClientCallId, Session);
                    }
                    else
                    {
                        throw new Exception("Unable to create contact record.");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        private void AddProgramParameter()
        {
            SessionManager.AddScriptURL(System.Web.HttpContext.Current.Request.Url.ToString(), HttpContext.Session);
            Dictionary<string, string> programParameter = new Dictionary<string, string>();
            Dictionary<string, string> scriptsParameter = new Dictionary<string, string>();
            string[] array = { "CALLID", "ANI", "PREVIOUS", "LEADID" };

            try
            {

                foreach (string key in Request.Form.Keys)
                {
                    if (Array.IndexOf(array, key.ToUpper()) != -1)
                        programParameter.Add(key, Request.Form[key]);
                    else
                        scriptsParameter.Add(key, Request.Form[key]);
                }

                foreach (string key in Request.QueryString.Keys)
                {
                    if (Array.IndexOf(array, key.ToUpper()) != -1)
                        programParameter.Add(key, Request.QueryString[key]);
                    else
                        scriptsParameter.Add(key, Request.QueryString[key]);
                }

                programParameter.Add("AgentName", SessionManager.GetAgentName(HttpContext.Session));
                SessionManager.StoreProgramParameter(programParameter, HttpContext.Session);
                SessionManager.StoreScriptParameter(scriptsParameter, HttpContext.Session);
            }
            catch
            {

            }
        }

        private string PopulateLItem(ScriptLI li)
        {
            StringWriter stringWriter = new StringWriter();

            if (!li.IsListItem)
            {
                using (HtmlTextWriter writer = new HtmlTextWriter(stringWriter))
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "dropdown");
                    writer.RenderBeginTag(HtmlTextWriterTag.Li);

                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "dropdown-toggle");
                    writer.AddAttribute("data-toggle", "dropdown");
                    writer.AddAttribute("role", "button");
                    writer.AddAttribute("aria - haspopup", "true");
                    writer.AddAttribute("aria-expanded", "false");
                    writer.RenderBeginTag(HtmlTextWriterTag.A);
                    writer.Write(li.Text);
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "caret");
                    writer.RenderBeginTag(HtmlTextWriterTag.Span);
                    writer.RenderEndTag(); // end span
                    writer.RenderEndTag(); // end A


                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "dropdown-menu");
                    writer.RenderBeginTag(HtmlTextWriterTag.Ul); // Begin #1

                    if (li.theUL != null)
                    {
                        foreach (ScriptLI items in li.theUL.LIArray)
                        {
                            writer.Write(PopulateLItem(items));

                        }
                    }

                    writer.RenderEndTag();
                    writer.RenderEndTag(); // end Li
                    return stringWriter.ToString();
                }
            }

            ScreenViewer.API.Elements.LinkController controller = new ScreenViewer.API.Elements.LinkController();
            var scriptLink = controller.GetScriptLink(Convert.ToDecimal(li.ElementID));
            var linkResponse = scriptLink as OkNegotiatedContentResult<ScriptLink>;


            using (HtmlTextWriter writer = new HtmlTextWriter(stringWriter))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "nav-item");
                writer.RenderBeginTag(HtmlTextWriterTag.Li); // Begin #1

                if (linkResponse.Content.LinkType == "Web" || linkResponse.Content.LinkType == "Document")
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Href, linkResponse.Content.LinkURL);
                }

                if (linkResponse.Content.LinkType == "Workflow")
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Href, "/Workflow/Display/" + linkResponse.Content.LinkTypeID.ToString());
                }


                if (linkResponse.Content.LinkType == "Section")
                {
                    //writer.AddAttribute(HtmlTextWriterAttribute.Href, "/Section/DefaultWithLayout/" + linkResponse.Content.LinkTypeID.ToString());
                    writer.AddAttribute(HtmlTextWriterAttribute.Href, "#");
                    writer.AddAttribute("id", string.Format("section_{0}", linkResponse.Content.LinkTypeID.ToString()));
                    writer.AddAttribute("onclick", string.Format("clickSection('{0}')", linkResponse.Content.LinkTypeID.ToString()));
                }

                if (linkResponse.Content.LinkNewWindow.Equals(true))
                {
                    writer.AddAttribute("target", "_blank");
                }

                writer.AddAttribute(HtmlTextWriterAttribute.Class, "nav-link");

                writer.RenderBeginTag(HtmlTextWriterTag.A);

                writer.Write(linkResponse.Content.LinkDesc);

                writer.RenderEndTag();
                writer.RenderEndTag();

                return stringWriter.ToString();

            }
        }
    }
}