using System;
using System.Configuration;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Results;
using System.Web.Mvc;
using ScreenViewer.API;
using ScreenViewer.Models;
using ScreenViewer.SessionControl;
using System.Text.RegularExpressions;
using ScreenViewer.API.ExternalData;
using ScreenViewer.Models.ExternalData;
using ScreenViewer;
using ScreenViewer.Data;
using ScreenViewer.Controllers;
using System.IO;
using System.Web.UI;
using System.Xml.Serialization;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using ScreenViewer.Authentication;

namespace ScreenViewer.Controllers
{
    public class DashboardController : Controller
    {
        // GET: Dashboard
        public ActionResult Index()
        {
            return View();
        }

        [ActionName("DisplayDashboard")]
        public ActionResult DisplayByName(string dashboardname)
        {
            Session["ReturnURL"] = System.Web.HttpContext.Current.Request.Url.PathAndQuery;

            if (!ScreenViewer.SignInHelper.ValidateSignIn())
                return RedirectToAction("Login", "Account", new { ReturnUrl = Session["ReturnURL"].ToString() });

            API.ScriptDashesController SDC = new ScriptDashesController();
            int dashboardId = SDC.GetDashboardId(dashboardname);
            AddProgramParameter();
            TempData["addparams"] = true;
            return RedirectToAction("Display", "Dashboard", new { id = dashboardId });
        }

        // GET: Dashboard/Details/5
        public ActionResult Display(int id)
        {
            ViewBag.DashBoardId = id.ToString();
            
            DashBoardDisplay wfDisplay = new DashBoardDisplay();

            API.ScriptDashesController SDC = new ScriptDashesController();
            
            var actionResult3 = SDC.GetScriptDash(id);

            ScriptDash scriptdash = null;
            if (actionResult3 != null)
            {
                var response = actionResult3 as OkNegotiatedContentResult<ScriptDash>;
                scriptdash = response.Content;
            }


            Session["layout"] = null;
            ViewBag.IsLoad = true;
            wfDisplay.sectionID = scriptdash.ScriptSectionID;

            string layoutname = scriptdash.ScreenLayout;


            if (scriptdash.ScriptMenuID != null)
            {
                wfDisplay.menuHTML = GetMenu((int)scriptdash.ScriptMenuID);
            }


            WorkflowLayoutsController WLC = new WorkflowLayoutsController();

            var actionResult2 = WLC.GetWorkflowLayoutString(layoutname, SessionControl.SessionManager.GetClientId(HttpContext.Session));
            if (actionResult2 != null && actionResult2 != actionResult2 as System.Web.Http.Results.NotFoundResult)
            {
                var response = actionResult2 as OkNegotiatedContentResult<string>;
                wfDisplay.Layout = response.Content;
            }
            else
            {
                if (Session["ReturnURL"] != null)
                    return RedirectToAction("Login", "Account", new { ReturnUrl = Session["ReturnURL"].ToString() });
                else
                    return RedirectToAction("Login", "Account");
            }

            return View("DashboardView", wfDisplay);
        }

        public ActionResult DisplayPartial(int id)
        {
            ViewBag.DashBoardId = id.ToString();

            DashBoardDisplay wfDisplay = new DashBoardDisplay();

            API.ScriptDashesController SDC = new ScriptDashesController();

            var actionResult3 = SDC.GetScriptDash(id);

            ScriptDash scriptdash = null;
            if (actionResult3 != null)
            {
                var response = actionResult3 as OkNegotiatedContentResult<ScriptDash>;
                scriptdash = response.Content;
            }


            Session["layout"] = null;
            ViewBag.IsLoad = true;
            wfDisplay.sectionID = scriptdash.ScriptSectionID;

            string layoutname = scriptdash.ScreenLayout;


            if (scriptdash.ScriptMenuID != null)
            {
                wfDisplay.menuHTML = GetMenu((int)scriptdash.ScriptMenuID);
            }


            WorkflowLayoutsController WLC = new WorkflowLayoutsController();

            var actionResult2 = WLC.GetWorkflowLayoutString(layoutname, SessionControl.SessionManager.GetClientId(HttpContext.Session));
            if (actionResult2 != null && actionResult2 != actionResult2 as System.Web.Http.Results.NotFoundResult)
            {
                var response = actionResult2 as OkNegotiatedContentResult<string>;
                wfDisplay.Layout = response.Content;
            }
            else
            {
                if (Session["ReturnURL"] != null)
                    return RedirectToAction("Login", "Account", new { ReturnUrl = Session["ReturnURL"].ToString() });
                else
                    return RedirectToAction("Login", "Account");
            }

            return PartialView("DashboardView", wfDisplay);
        }

        [System.Web.Mvc.HttpPost]
        public ActionResult myForm(FormCollection collection)
        {
            RetrieveResponses(collection);
            return DisplayPartial(Convert.ToInt32(Request.Form["hdnDashBoardId"]));
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

                SessionManager.StoreProgramParameter(programParameter, HttpContext.Session);
                SessionManager.StoreScriptParameter(scriptsParameter, HttpContext.Session);

                Session["ClientId"] = ScreenViewer.ClientHelper.GetClientIdByUserID(SessionManager.GetUserId(HttpContext.Session));
            }
            catch
            {

            }
        }

        public string GetMenu(int MenuID)
        {
            ScreenViewer.API.ProjectController PC = new API.ProjectController();
            var scriptMenu = PC.GetScriptProjectMenu(MenuID);
            var menuResponse = scriptMenu as OkNegotiatedContentResult<ScriptMenu>;

            ScriptUL ul;
            using (StringReader sr = new StringReader(menuResponse.Content.MenuXML))
            {
                XmlSerializer xs = new XmlSerializer(typeof(ScriptUL));
                ul = (ScriptUL)xs.Deserialize(sr);

                // SessionControl.SessionManager.StoreMenu(HttpContext.Session, lstMenuItem);

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

            return stringWriter.ToString();

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

                    foreach (ScriptLI items in li.theUL.LIArray)
                    {
                        writer.Write(PopulateLItem(items));

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

                if (linkResponse.Content.LinkType == "Web")
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Href, linkResponse.Content.LinkURL);
                }

                if (linkResponse.Content.LinkType == "Workflow")
                {

                    writer.AddAttribute(HtmlTextWriterAttribute.Href, "/Workflow/Display/" + linkResponse.Content.LinkTypeID.ToString());
                }


                if (linkResponse.Content.LinkType == "Section")
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Href, "/Section/DefaultWithLayout/" + linkResponse.Content.LinkTypeID.ToString());
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
                            case "Link":
                                //PopUps.Add(scriptAction.ActionXML);
                                break;
                            case "Set Question Value":
                                WorkflowHelper.SetQuestionValue(HttpContext.Session, scriptAction.ActionXML);
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
                        }
                    }
                }
            }
        }


    }

    public struct ItemCounts
    {
        public string itemCode;
        public Int32 itemQuantity;
        public string ownerOIS;
    }
}
