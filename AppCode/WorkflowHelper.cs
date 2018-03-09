using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;
using System.IO;
using Microsoft.CSharp.RuntimeBinder;
using System.Text.RegularExpressions;
using System.Web.Http.Results;
using System.Collections;
using System.Reflection;
using System.Text;
using System.Net.Mail;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;
using ScreenViewer.API;
using ScreenViewer.Models;
using ScreenViewer.Data;
using ScreenViewer.SessionControl;
using ScreenViewer.API.ExternalData;
using ScreenViewer.API.SendText;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Twilio;
using System.Threading.Tasks;

namespace ScreenViewer
{
    public static class WorkflowHelper
    {
        public static void APIGET(HttpSessionStateBase Session, string actionXML)
        {
            WorkFlowActionXMLObject wfXML = DeserialezeWFActionXML(actionXML);
            string webAPI = SPutilities.ReplaceObjectsandQuestions(Session, Regex.Split(wfXML.SetValue, "~~")[0], false);
            DataObjectLoader doLoader = new API.ExternalData.DataObjectLoader();
            doLoader.LoadDataObjectFromWebAPI(Convert.ToInt32(wfXML.SetObj), webAPI, Session);
        }

        public static void APIPOST(HttpSessionStateBase Session, string actionXML)
        {
            WorkFlowActionXMLObject wfXML = DeserialezeWFActionXML(actionXML);
            string webAPI = SPutilities.ReplaceObjectsandQuestions(Session, Regex.Split(wfXML.SetValue, "~~")[0], false);

            DataObjectController dataObjectController = new DataObjectController();
            object loadObject = null;
            var actionResult = dataObjectController.GetDataObject(Convert.ToInt32(wfXML.SetObj), loadObject);

            if (actionResult != null)
            {
                var objectResponse = actionResult as OkNegotiatedContentResult<DataObjects>;
                DataObjects scriptDataObject = (DataObjects)SessionControl.SessionManager.GetDataObject(Session, objectResponse.Content.DOName);

                var myType = GenerateClass.CompileResultType(scriptDataObject.Details);
                var myObject = Activator.CreateInstance(myType);

                scriptDataObject.ReverseObjectMatch(string.Empty, myObject);
                
                HttpClient client = new HttpClient();
                HttpResponseMessage response = client.PostAsync(webAPI, myObject, new JsonMediaTypeFormatter()).Result;

                if (response.IsSuccessStatusCode)
                {
                    var theresponse = response.Content.ReadAsStringAsync();
                    string json = theresponse.Result.Replace("[", "").Replace("]", "");
                    var converter = new ExpandoObjectConverter();
                    var obj = JsonConvert.DeserializeObject(json);
                    JToken token = JObject.Parse(obj.ToString());

                    if (token.SelectToken("Status").ToString().Equals("SUCCESS"))
                    {
                        SessionManager.StoreProgramParameter("APIError", "No", Session);
                    }
                    else if (token.SelectToken("Status").ToString().Equals("FAILED"))
                    {
                        SessionManager.StoreProgramParameter("APIError", token.SelectToken("ErrorMessage1").ToString(), Session);
                    }
                }
                else
                {
                    SessionManager.StoreProgramParameter("APIError", response.ReasonPhrase, Session);
                }
            }
        }

        public static void APIGETPLAIN(HttpSessionStateBase Session, string actionXML)
        {
            WorkFlowActionXMLObject wfXML = DeserialezeWFActionXML(actionXML);
            string webAPI = SPutilities.ReplaceObjectsandQuestions(Session, Regex.Split(wfXML.SetValue, "~~")[0], false);

            if (webAPI.Contains("[DISPO]"))
            {
                int contactID = SessionManager.GetContactId(Session);
                ScreenViewer.API.ExternalData.ContactRecordController CRC = new ContactRecordController();
                var actionResult = CRC.GetContactRecordByContactId(contactID);

                var cresponse = actionResult as OkNegotiatedContentResult<ContactRecord>;
                ContactRecord cRecord = cresponse.Content;

                webAPI = webAPI.Replace("[DISPO]", cRecord.DispositionCode);
            }

            HttpClient client = new HttpClient();
            //client.BaseAddress = new Uri(webAPIbaseaddress);
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = client.GetAsync(webAPI).Result;

            if (response.IsSuccessStatusCode)
            {
                var theresponse = response.Content.ReadAsStringAsync();
                //string json = theresponse.Result.Replace("[", "").Replace("]", "");
                string json = theresponse.Result;
                var converter = new ExpandoObjectConverter();
                var obj = JsonConvert.DeserializeObject(json);
                JToken token = JObject.Parse(obj.ToString());

                if (token.SelectToken("Status").ToString().Equals("SUCCESS"))
                {
                    SessionManager.StoreProgramParameter("TMCDISPO", "TMC Dispo Completed", Session);
                }
                else if (token.SelectToken("Status").ToString().Equals("FAILED"))
                {
                    SessionManager.StoreProgramParameter("TMCDISPO", token.SelectToken("ErrorMessage1").ToString(), Session);
                }


                //Example Multi Collection
                //string json = "[{\"Id\":\"00Qj000000IyrXzEAJ\",\"Company\":\"ZINGERS BAR AND GRILL\",\"Title\":null,\"Street\":\"12700 Hill Country Blvd\",\"City\":\"AUSTIN\",\"State\":\"Texas\",\"Country\":\"United States\",\"PostalCode\":\"78738\",\"Phone\":\"(512) 524-2430\",\"MobilePhone\":null,\"Email\":\"alan@zingersbarandgrill.com\",\"FirstName\":\"ALAN\",\"LastName\":\"MCARTHUR\",\"LeadSource\":\"List\",\"Status\":\"Open\",\"Lead_MID__c\":\"650000006747920\",\"Non_Sales_Disposition__c\":null,\"Products_Presented__c\":null,\"Card_Termination_Date__c\":\"2015-07-14\",\"Recent_or_Existing_Merchant_Acct__c\":null,\"Est_Annual_Process_Volume__c\":null,\"Average_Ticket__c\":null,\"Current_Processor__c\":null,\"Name_of_Existing_Equipment_softward__c\":null},{\"Id\":\"00Qj000000IyrXzEAJ\",\"Company\":\"ZINGERS BAR AND GRILL\",\"Title\":null,\"Street\":\"12700 Hill Country Blvd\",\"City\":\"AUSTIN\",\"State\":\"Texas\",\"Country\":\"United States\",\"PostalCode\":\"78738\",\"Phone\":\"(512) 524-2430\",\"MobilePhone\":null,\"Email\":\"alan@zingersbarandgrill.com\",\"FirstName\":\"ALAN\",\"LastName\":\"MCARTHUR\",\"LeadSource\":\"List\",\"Status\":\"Open\",\"Lead_MID__c\":\"650000006747920\",\"Non_Sales_Disposition__c\":null,\"Products_Presented__c\":null,\"Card_Termination_Date__c\":\"2015-07-14\",\"Recent_or_Existing_Merchant_Acct__c\":null,\"Est_Annual_Process_Volume__c\":null,\"Average_Ticket__c\":null,\"Current_Processor__c\":null,\"Name_of_Existing_Equipment_softward__c\":null}]";
                //Example Single Collection
                //string json = "[{\"Id\":\"00Qj000000IyrXzEAJ\",\"Company\":\"ZINGERS BAR AND GRILL\",\"Title\":null,\"Street\":\"12700 Hill Country Blvd\",\"City\":\"AUSTIN\",\"State\":\"Texas\",\"Country\":\"United States\",\"PostalCode\":\"78738\",\"Phone\":\"(512) 524-2430\",\"MobilePhone\":null,\"Email\":\"alan@zingersbarandgrill.com\",\"FirstName\":\"ALAN\",\"LastName\":\"MCARTHUR\",\"LeadSource\":\"List\",\"Status\":\"Open\",\"Lead_MID__c\":\"650000006747920\",\"Non_Sales_Disposition__c\":null,\"Products_Presented__c\":null,\"Card_Termination_Date__c\":\"2015-07-14\",\"Recent_or_Existing_Merchant_Acct__c\":null,\"Est_Annual_Process_Volume__c\":null,\"Average_Ticket__c\":null,\"Current_Processor__c\":null,\"Name_of_Existing_Equipment_softward__c\":null}]";
                //Example No Collection
                //string json = "{\"Id\":\"00Qj000000IyrXzEAJ\",\"Company\":\"ZINGERS BAR AND GRILL\",\"Title\":null,\"Street\":\"12700 Hill Country Blvd\",\"City\":\"AUSTIN\",\"State\":\"Texas\",\"Country\":\"United States\",\"PostalCode\":\"78738\",\"Phone\":\"(512) 524-2430\",\"MobilePhone\":null,\"Email\":\"alan@zingersbarandgrill.com\",\"FirstName\":\"ALAN\",\"LastName\":\"MCARTHUR\",\"LeadSource\":\"List\",\"Status\":\"Open\",\"Lead_MID__c\":\"650000006747920\",\"Non_Sales_Disposition__c\":null,\"Products_Presented__c\":null,\"Card_Termination_Date__c\":\"2015-07-14\",\"Recent_or_Existing_Merchant_Acct__c\":null,\"Est_Annual_Process_Volume__c\":null,\"Average_Ticket__c\":null,\"Current_Processor__c\":null,\"Name_of_Existing_Equipment_softward__c\":null}";
                //Example Collection for DropDown
                //string json = "[{\"Name\":\"Do Not Call\",\"Value\":\"Do_Not_Call\"},{\"Name\":\"Call Back\",\"Value\":\"Call_Back\"},{\"Name\":\"Fax Machine\",\"Value\":\"Fax_Machine\"},{\"Name\":\"Wrong Number\",\"Value\":\"Wrong_Number\"}]";
            }

            if (response != null)
            {
                response.Dispose();
                client.Dispose();
            }
        }
        public static void APIPOSTCOMPLEX(HttpSessionStateBase Session, string actionXML)
        {

            WorkFlowActionXMLObject wfXML = DeserialezeWFActionXML(actionXML);
            string webAPI = SPutilities.ReplaceObjectsandQuestions(Session, Regex.Split(wfXML.SetValue, "~~")[0], false);
            bool iscollection = false;
            bool matchobject = false;

            DataObjectController dataObjectController = new DataObjectController();
            object loadObject = null;
            var actionResult = dataObjectController.GetDataObject(Convert.ToInt32(wfXML.SetObj), loadObject);

            if (actionResult != null)
            {
                var objectResponse = actionResult as OkNegotiatedContentResult<DataObjects>;
                DataObjects scriptDataObject = (DataObjects)SessionControl.SessionManager.GetDataObject(Session, objectResponse.Content.DOName);
                Uri suri = new Uri(webAPI);

                ScriptObject SO = dataObjectController.GetDataObject(objectResponse.Content.DOName + "_Response");

                if (SO == null)
                {
                    iscollection = false;
                    matchobject = false;
                }
                else
                {
                    iscollection = SO.IsCollection;
                    matchobject = true;
                }

                string thebase = suri.GetLeftPart(UriPartial.Authority);

                string therest = suri.GetComponents(UriComponents.PathAndQuery, UriFormat.SafeUnescaped);

                HttpRequestMessage HRM = new HttpRequestMessage(HttpMethod.Post, therest);


                var postData = new List<KeyValuePair<string, string>>();
                List<String> theobs = new List<string>();

                List<Dictionary<string,string>> mydic = new List<Dictionary<string,string>>();
                bool usejson = false;
                string jsonbody = "";

                foreach (var field in scriptDataObject.Details)
                {

                    if (field.DetailName == "OBJECT_JSONDATA")
                    {

                        string ID = field.DetailValue.ToString();

                        ScreenViewer.API.Elements.TextHTMLController TC = new API.Elements.TextHTMLController();

                        var actionResult2 = TC.GetScriptHTML(Convert.ToDecimal(ID));

                        var tresponse = actionResult2 as OkNegotiatedContentResult<Data.ScriptHTML>;
                        Data.ScriptHTML theText = tresponse.Content;

                        jsonbody = SPutilities.ReplaceObjectsandQuestions_blanks(Session, theText.HTMLContent, true);


                    }

                    if (field.DetailName.Substring(0, 5).ToUpper() == "FORM_")
                    {
                        if (field.DetailValue == null)
                            continue;
                        string fname = field.DetailName.Substring(5);
                        object responsestr = DataObjects.ReturnValue(scriptDataObject.DOName + "." + field.DetailName, Session);

                        string theval = "";

                        if (responsestr != null)
                        {
                            theval = responsestr.ToString();
                            postData.Add(new KeyValuePair<string, string>(fname, theval));
                        }




                    }
                    if (field.DetailName.Substring(0, 7).ToUpper() == "HEADER_")
                    {
                        string fname = field.DetailName.Substring(7);

                        object responsestr = DataObjects.ReturnValue(scriptDataObject.DOName + "." + field.DetailName, Session);

                        string theval = "";

                        if (responsestr != null)
                        {
                            theval = responsestr.ToString();
                        }
                        HRM.Headers.Add(fname, theval);
                    }

                }

                if (usejson)
                {
                    HRM.Content = new StringContent(jsonbody, Encoding.UTF8, "application/json"); 

                }
                else
                {
                    HRM.Content = new FormUrlEncodedContent(postData);
                }

                //ScreenViewer.AppCode.MessageHandler MH = new AppCode.MessageHandler();

                //var response = MH.SendPost(HRM, suri);


                using (var client = new HttpClient())
                {

                    client.BaseAddress = new Uri(thebase);

                    HttpResponseMessage response = null;
                    try
                    {
                        response = client.SendAsync(HRM, HttpCompletionOption.ResponseContentRead).Result;
                    }
                    catch (Exception e)
                    {
                        DataObjects.SetValue(scriptDataObject.DOName + ".RESPONSE_Status", "Error", Session);
                        DataObjects.SetValue(scriptDataObject.DOName + "." + ".RESPONSE_Error", e.Message, Session);
                        return;
                    }
                    string res = "";

                    if (response.IsSuccessStatusCode == false)
                    {
                        DataObjects.SetValue(scriptDataObject.DOName + ".RESPONSE_Status", response.StatusCode.ToString(), Session);
                        DataObjects.SetValue(scriptDataObject.DOName + "." + ".RESPONSE_Error", response.ReasonPhrase.ToString(), Session);
                    }
                    else

                    {
                        if (matchobject == false)
                        {
                            SessionManager.StoreProgramParameter(scriptDataObject.DOName + "_ResponseBody", response.ToString(), Session);
                            if (response.Content != null)
                            {
                                SessionManager.StoreProgramParameter(scriptDataObject.DOName + "_ResponseContent", response.Content.ToString(), Session);
                            }

                        }
                        else
                        {
                            using (HttpContent content = response.Content)
                            {
                                // ... Read the string.
                                Task<string> result = content.ReadAsStringAsync();
                                res = result.Result;
                                DataObjects.SetValue(scriptDataObject.DOName + ".RESPONSE_Status", result.Status.ToString(), Session);
                                if (result.Exception != null)
                                { DataObjects.SetValue(scriptDataObject.DOName + "." + ".RESPONSE_Error", result.Exception.ToString(), Session); }



                                if (iscollection)
                                {
                                    var obj = JsonConvert.DeserializeObject(res);
                                    DataObjectController DOC = new DataObjectController();
                                    ScriptObject scriptObject = DOC.GetDataObject(scriptDataObject.DOName + "_Response");
                                    var ObjectResult = DOC.GetDataObjectDictionary(scriptObject.ScriptObjectID, obj);
                                    DataObjects DisplayObject = (ObjectResult as OkNegotiatedContentResult<DataObjects>).Content;
                                    SessionManager.StoreDataObject(Session, DisplayObject.DOName, DisplayObject);

                                }
                                else
                                {

                                    dynamic jsonObject = JsonConvert.DeserializeObject<dynamic>(res);

                                    if (jsonObject.First == null)
                                    {
                                        var x = jsonObject;
                                        foreach (var y in x)
                                        {
                                            try
                                            {
                                                string key = y.Name;
                                                string value = y.Value;

                                                DataObjects.SetValue(scriptDataObject.DOName + "_Response" + "." + key, value, Session);
                                            }
                                            catch { }
                                        }

                                    }
                                    else
                                    {
                                        var x = jsonObject.First;
                                        foreach (var y in x)
                                        {
                                            try
                                            {
                                                string key = y.Name;
                                                string value = y.Value;

                                                DataObjects.SetValue(scriptDataObject.DOName + "_Response" + "." + key, value, Session);
                                            }
                                            catch { }
                                        }

                                    }

                                }
                            }
                        }
                    }
                }

            }

        }
        public static void APIPUT(HttpSessionStateBase Session, string actionXML)
        {
            WorkFlowActionXMLObject wfXML = DeserialezeWFActionXML(actionXML);
            string webAPI = SPutilities.ReplaceObjectsandQuestions(Session, Regex.Split(wfXML.SetValue, "~~")[0], false);

            DataObjectController dataObjectController = new DataObjectController();
            object loadObject = null;
            var actionResult = dataObjectController.GetDataObject(Convert.ToInt32(wfXML.SetObj), loadObject);

            if (actionResult != null)
            {
                var objectResponse = actionResult as OkNegotiatedContentResult<DataObjects>;
                DataObjects scriptDataObject = (DataObjects)SessionControl.SessionManager.GetDataObject(Session, objectResponse.Content.DOName);

                var myType = GenerateClass.CompileResultType(scriptDataObject.Details);
                var myObject = Activator.CreateInstance(myType);

                scriptDataObject.ReverseObjectMatch(string.Empty, myObject);

                HttpClient client = new HttpClient();
                HttpResponseMessage response = client.PutAsync(webAPI, myObject, new JsonMediaTypeFormatter()).Result;

                if (response.IsSuccessStatusCode)
                {
                    var theresponse = response.Content.ReadAsStringAsync();
                    string json = theresponse.Result.Replace("[", "").Replace("]", "");
                    var converter = new ExpandoObjectConverter();
                    var obj = JsonConvert.DeserializeObject(json);
                    JToken token = JObject.Parse(obj.ToString());

                    if (token.SelectToken("Status").ToString().Equals("SUCCESS"))
                    {
                        SessionManager.StoreProgramParameter("APIError", "No", Session);
                    }
                    else if (token.SelectToken("Status").ToString().Equals("FAILED"))
                    {
                        SessionManager.StoreProgramParameter("APIError", token.SelectToken("ErrorMessage1").ToString(), Session);
                    }
                }
                else
                {
                    SessionManager.StoreProgramParameter("APIError", response.ReasonPhrase, Session);
                }
            }
        }

        public static void GeneratePDF(HttpSessionStateBase Session, string actionXML)
        {
            HttpClient client = null;
            HttpResponseMessage response = null;
            WorkFlowActionXMLObject wfXML = null;
            List<QuestVal> questvals = null;
            Dictionary<string, object> fields = null;

            try
            {
                wfXML = DeserialezeWFActionXML(actionXML);
                questvals = SessionManager.GetQuestionResponses(Session);
                fields = new Dictionary<string, object>();

                fields.Add("ContactId", SessionManager.GetContactId(Session).ToString());
                if (questvals != null && questvals.Count > 0)
                {
                    foreach (var item in questvals)
                    {
                        List<string> keys = Regex.Split(item.QuestKeyCodes, "\\|\\|").ToList();

                        foreach (string key in keys)
                        {
                            if (key.StartsWith(wfXML.SetObj))
                                fields.Add(key.Replace(wfXML.SetObj, string.Empty), item.Response);
                        }

                    }
                }

                var exo = new System.Dynamic.ExpandoObject();

                foreach (var field in fields)
                {
                    ((IDictionary<String, Object>)exo).Add(field.Key, field.Value);
                }

                client = new HttpClient();
                response = client.PostAsync(wfXML.SetValue, exo, new JsonMediaTypeFormatter()).Result;

                if (response.IsSuccessStatusCode)
                {
                    var theresponse = response.Content.ReadAsStringAsync();
                    string json = theresponse.Result.Replace("[", "").Replace("]", "");
                    var converter = new ExpandoObjectConverter();
                    var obj = JsonConvert.DeserializeObject(json);
                    JToken token = JObject.Parse(obj.ToString());

                    if (token.SelectToken("Status").ToString().Equals("SUCCESS"))
                    {
                        SessionManager.StoreProgramParameter("APIError", "No", Session);
                        SessionManager.StoreProgramParameter("PDFUrl", token.SelectToken("StatusDescription").ToString(), Session);
                    }
                    else if (token.SelectToken("Status").ToString().Equals("FAILED"))
                    {
                        SessionManager.StoreProgramParameter("APIError", token.SelectToken("ErrorMessage1").ToString(), Session);
                    }
                }
                else
                {
                    SessionManager.StoreProgramParameter("APIError", response.ReasonPhrase, Session);
                }
            }
            catch (Exception ex)
            {
                SessionManager.StoreProgramParameter("APIError", ex.Message, Session);
            }
        }

        public static void SendText(HttpSessionStateBase Session, string actionXML)
        {
            WorkFlowActionXMLObject wfXML = DeserialezeWFActionXML(actionXML);
            string thecell = "+1" + SPutilities.ReplaceObjectsandQuestions(Session, wfXML.SetObj, false);
            string themessage = SPutilities.ReplaceObjectsandQuestions(Session, wfXML.SetValue, false);
            TwillioSMSController twill = new TwillioSMSController();
            twill.Send("+16464193635", thecell,themessage);
        }

        public static void SetQuestionValue(HttpSessionStateBase Session, string actionXML)
        {
            WorkFlowActionXMLObject wfxml2 = DeserialezeWFActionXML(actionXML);
            string thevalue = string.Empty;

            if (wfxml2.SetObj.Contains("|"))
            {
                string[] qid = wfxml2.SetObj.Split('|');
                SessionManager.AddUpdateQuestion(qid, wfxml2.SetValue, wfxml2.SetValue, Session);
            }
            else
            {
                thevalue = SPutilities.ReplaceObjectsandQuestions(Session, wfxml2.SetValue, false);
                SessionManager.AddUpdateQuestion(wfxml2.SetObj, thevalue, thevalue, Session);
            }
        }

        public static void SetQuestionText(HttpSessionStateBase Session, string actionXML)
        {
            WorkFlowActionXMLObject wfxml2 = DeserialezeWFActionXML(actionXML);
            string thevalue = string.Empty;

            if (wfxml2.SetObj.Contains("|"))
            {
                string[] qid = wfxml2.SetObj.Split('|');
                SessionManager.AddUpdateQuestion(qid, wfxml2.SetValue, wfxml2.SetValue, Session);
            }
            else
            {
                thevalue = SPutilities.ReplaceObjectsandQuestions(Session, wfxml2.SetValue, true);
                SessionManager.AddUpdateQuestion(wfxml2.SetObj, thevalue, thevalue, Session);
            }
        }

        public static void SetParameter(HttpSessionStateBase Session, string actionXML)
        {
            WorkFlowActionXMLObject wfxml2 = DeserialezeWFActionXML(actionXML);
            string thevalue = SPutilities.ReplaceObjectsandQuestions(Session, wfxml2.SetValue, false);
            SessionManager.StoreProgramParameter(wfxml2.SetObj, thevalue, Session);
        }

        public static void SetItemKey(HttpSessionStateBase Session, string actionXML)
        {
            WorkFlowActionXMLObject wfxml3 = DeserialezeWFActionXML(actionXML);
            string thevalue3 = SPutilities.ReplaceObjectsandQuestions(Session, wfxml3.SetValue, false);
            SessionManager.SetItemKey(Session, wfxml3.SetObj, thevalue3, "A");
        }

        public static void RemoveItemKey(HttpSessionStateBase Session, string actionXML)
        {
            WorkFlowActionXMLObject wfxml4 = DeserialezeWFActionXML(actionXML);
            SessionManager.RemoveItemKey(Session, wfxml4.SetObj, "A");
        }

        public static void InsertIDRQueue(HttpSessionStateBase Session, string actionXML)
        {
            DateTime timeUtc = DateTime.UtcNow;

            WorkFlowActionXMLObject wfxml3 = DeserialezeWFActionXML(actionXML);
            string serviceQueue = SPutilities.ReplaceObjectsandQuestions(Session, wfxml3.SetValue, false);

            IDRServiceQueue queue = new IDRServiceQueue();
            queue.ContactId = SessionManager.GetContactId(Session);
            queue.ClientCallId = SessionManager.GetProgramParameterByKey("CallID", Session);
            queue.ServiceQueue = serviceQueue;
            queue.Status = "OPEN";

            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(ConfigurationManager.AppSettings["timeZone"]);
            DateTime dateTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, timeZone);

            queue.CreatedDate = dateTime;
            queue.ModifiedDate = dateTime;
            queue.ClientId = SessionManager.GetClientId(Session);

            IDRController controller = new IDRController();
            controller.PostIDRServiceQueue(queue);
        }

        public static void UpdateIDRQueue(HttpSessionStateBase Session, string actionXML)
        {
            WorkFlowActionXMLObject wfxml4 = DeserialezeWFActionXML(actionXML);
            string status = SPutilities.ReplaceObjectsandQuestions(Session, wfxml4.SetValue, false);
            string callID = SessionManager.GetScriptParameterByKey("PreviousCallID", Session);

            IDRController controller = new IDRController();
            var actionResult = controller.GetIDRServiceQueue(Convert.ToInt32(callID));
            var response = actionResult as OkNegotiatedContentResult<IDRServiceQueue>;
            IDRServiceQueue queue = response.Content;

            DateTime timeUtc = DateTime.UtcNow;
            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(ConfigurationManager.AppSettings["timeZone"]);
            DateTime dateTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, timeZone);

            if (queue != null)
            {
                queue.Status = status;
                queue.ModifiedDate = dateTime;
                controller.PutIDRServiceQueue(queue);
            }
        }

        public static void UpdateObject(HttpSessionStateBase Session, string actionXML)
        {
            WorkFlowActionXMLObject wfxml1 = DeserialezeWFActionXML(actionXML);
            string objectName = wfxml1.SetObj;
            string[] objectFields = wfxml1.SetValue.Split('~');
            string objectFieldXML = string.Empty;

            foreach (string field in objectFields)
            {
                string[] fieldArray = field.Split('|');
                objectFieldXML = string.Format("<ActionXML><SetObj>{0}.{1}</SetObj><SetValue>{2}</SetValue></ActionXML>", objectName, fieldArray[0], fieldArray[1]);
                UpdateDataObjectField(Session, objectFieldXML);
            }
        }

        public static void SaveCRMLead(HttpSessionStateBase Session)
        {
            string objectId = SessionManager.GetProgramParameterByKey("CRMLeadObjectId", Session);

            DateTime timeUtc = DateTime.UtcNow;
            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(ConfigurationManager.AppSettings["timeZone"]);
            DateTime dateTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, timeZone);

            if (!string.IsNullOrEmpty(objectId))
            {
                DataObjectController dataObjectController = new DataObjectController();
                object loadObject = null;
                var actionResult = dataObjectController.GetDataObject(Convert.ToInt32(objectId), loadObject);

                if (actionResult != null)
                {
                    var objectResponse = actionResult as OkNegotiatedContentResult<DataObjects>;
                    DataObjects scriptDataObject = (DataObjects)SessionControl.SessionManager.GetDataObject(Session, objectResponse.Content.DOName);

                    var myType = GenerateClass.CompileResultType(scriptDataObject.Details);
                    dynamic LeadObject = Activator.CreateInstance(myType);

                    scriptDataObject.ReverseObjectMatch(string.Empty, LeadObject);

                    if (IsPropertyExist(LeadObject, "LeadRecordId"))
                    {
                        LeadRecordController leadController = new LeadRecordController();
                        LeadRecord lead;
                        var actionresult = leadController.GetLeadRecord(LeadObject.LeadRecordId);
                        if (actionresult != null)
                        {
                            var response = actionresult as OkNegotiatedContentResult<LeadRecord>;
                            lead = response.Content;
                            CreateMapping(lead, LeadObject);
                            lead.ModifiedDate = dateTime;
                            leadController.PutLeadRecord(LeadObject.LeadRecordId, lead);
                        }
                    }
                }
            }
        }

        public static bool IsPropertyExist(dynamic settings, string name)
        {
            return settings.GetType().GetProperty(name) != null;
        }

        public static void UpdateDataObjectField(HttpSessionStateBase Session, string actionXML)
        {
            WorkFlowActionXMLObject wfxml1 = DeserialezeWFActionXML(actionXML);
            string thevalue1 = SPutilities.ReplaceObjectsandQuestions(Session, wfxml1.SetValue, false);
            DataObjects.SetValue(wfxml1.SetObj, thevalue1, Session);
        }
        
        public static void Email(HttpSessionStateBase Session, string actionXML)
        {
            WorkFlowActionXMLObject wfxml2 = DeserialezeWFActionXML(actionXML);
            string[] theObject = wfxml2.SetObj.Split('|');

            string emailTo = theObject[0];
            string emailCC = theObject[1];
            string emailSubject = theObject[2];
            string emailAttachment = theObject[3];

            emailTo = SPutilities.ReplaceObjectsandQuestions(Session, emailTo, false);
            emailCC = SPutilities.ReplaceObjectsandQuestions(Session, emailCC, false);
            emailSubject = SPutilities.ReplaceObjectsandQuestions(Session, emailSubject, false);
            string thevalue = SPutilities.ReplaceObjectsandQuestions(Session, wfxml2.SetValue, false);

            if (thevalue.Contains("(parameter not found)"))
                thevalue = thevalue.Replace("(parameter not found)", string.Empty);

            StringBuilder message = new StringBuilder();
            message.Append("<html>\r\n");
            message.Append("<head>\r\n");
            message.Append("	<style>\r\n");
            message.Append("		body {font-family: verdana; color: black; font-size: x-small;}\r\n");
            message.Append("		li {list-style: none; padding-bottom: 10px;}\r\n");
            message.Append("		li.header {font-weight: bold;}\r\n");
            message.Append("		li.content_sub_header {font-style: italic; padding-left: 25px;}\r\n");
            message.Append("		li.content {padding-left: 50px;}\r\n");
            message.Append("		div.header {font-weight: bold;}\r\n");
            message.Append("	</style>\r\n");
            message.Append("</head>\r\n");
            message.Append("	<body>\r\n");
            message.Append(thevalue);
            message.Append("	</body>\r\n");
            message.Append("</html>\r\n");

            using (SmtpClient client = new SmtpClient())
            {
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(ConfigurationManager.AppSettings["emailLogSender"], ConfigurationManager.AppSettings["emailLogSenderName"]);
                mail.To.Add(emailTo);

                if (!string.IsNullOrEmpty(emailCC))
                {
                    mail.CC.Add(emailCC);
                }

                mail.Subject = emailSubject;
                mail.BodyEncoding = System.Text.Encoding.UTF8;
                mail.IsBodyHtml = true;
                mail.Priority = MailPriority.Normal;

                string[] attachments = emailAttachment.Split(',');
                foreach(string item in attachments)
                {
                    try
                    {
                        string file = SPutilities.ReplaceObjectsandQuestions(Session, item, false);
                        mail.Attachments.Add(new Attachment(System.Web.HttpContext.Current.Server.MapPath(file)));
                    }
                    catch (Exception ex) { }
                }
                
                mail.Body = message.ToString();
                client.Send(mail);
            }
        }

        public static bool SaveProgress(HttpSessionStateBase Session)
        {
            ContactRecordDetail contactRecordDetail = null;
            OrderDetail orderDetail = null;
            List<QuestVal> questvals = null;
            List<ItemOrdered> collectionOrder = null;
            //ArrayList collection = null;
            //ArrayList ocollection = null;
            List<ContactRecordDetail> collection = null;
            List<OrderDetail> ocollection = null;
            decimal OrderID = 0;
            int contactID = SessionManager.GetContactId(Session);

            try
            {
                // Instantiate local variable;
                collection = new List<ContactRecordDetail>();
                ocollection = new List<OrderDetail>();
                questvals = SessionManager.GetQuestionResponses(Session);
                collectionOrder = SessionManager.GetItemsOrdered(Session);

                if (questvals != null && questvals.Count > 0)
                {
                    foreach (var item in questvals)
                    {
                        bool ignoreQuest = false;
                        if (item.QuestKeyCodes != "" && item.QuestKeyCodes != null)
                        {
                            List<string> keys = Regex.Split(item.QuestKeyCodes, "\\|\\|").ToList();
                            if (keys.IndexOf("Ignore") >= 0)
                            {
                                ignoreQuest = true;
                            }
                        }

                        if (!ignoreQuest)
                        {
                            contactRecordDetail = new ContactRecordDetail();
                            contactRecordDetail.QuestionId = Convert.ToInt32(item.QuestionID);
                            contactRecordDetail.QuestionText = item.QuestionText;
                            contactRecordDetail.QuestionResponseValue = item.Response;
                            contactRecordDetail.ContactId = contactID; //contactRecord.ContactID;
                            contactRecordDetail.QuestionResponseText = item.DisplayResponse;
                            contactRecordDetail.QuestionKeys = item.QuestKeyCodes;
                            collection.Add(contactRecordDetail);
                        }
                    }

                    //ICollection<ContactRecordDetail> collectionDetail = collection.<ContactRecordDetail>();
                    ScreenViewer.API.ExternalData.ContactRecordController CRC = new ContactRecordController();
                    CRC.PutContactRecord(collection);
                }

                if (collectionOrder != null && collectionOrder.Count > 0)
                {
                    decimal oid = SessionManager.GetOrderId(Session);
                    if (oid != 0)
                    {
                        OrderID = oid;
                    }
                    else
                    {
                        OrderID = CreateOrder(contactID, Session);
                    }

                    foreach (var orderitem in collectionOrder)
                    {
                        orderDetail = new OrderDetail();
                        orderDetail.OrderId = (int)OrderID;
                        orderDetail.ItemCode = orderitem.ItemCode;
                        orderDetail.ItemName = orderitem.ItemName;
                        orderDetail.ItemQuantity = orderitem.ItemQuantity;
                        orderDetail.ItemPrice = orderitem.ItemPrice;
                        orderDetail.ItemShippingPrice = orderitem.ItemShipping;
                        orderDetail.ItemKeys = orderitem.SetKeys;
                        ocollection.Add(orderDetail);
                    }

                    //ICollection<OrderDetail> orderCollection = ocollection.ToList<OrderDetail>();
                    ScreenViewer.API.ExternalData.OrderController OC = new API.ExternalData.OrderController();
                    OC.PutOrderDetails(ocollection);
                }

                if (Convert.ToBoolean(ConfigurationManager.AppSettings["saveNavigation"]))
                    SaveContactNavigation(Session);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static void DispositionSale(HttpSessionStateBase Session, string actionXML)
        {
            DateTime timeUtc = DateTime.UtcNow;
            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(ConfigurationManager.AppSettings["timeZone"]);
            DateTime dateTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, timeZone);

            WorkFlowActionXMLObject wfXMLObject = DeserialezeWFActionXML(actionXML);
            string dispositionValue = SPutilities.ReplaceObjectsandQuestions(Session, wfXMLObject.SetValue, false);

            int contactID = SessionManager.GetContactId(Session);
            ScreenViewer.API.ExternalData.ContactRecordController CRC = new ContactRecordController();
            var actionResult = CRC.GetContactRecordByContactId(contactID);

            var response = actionResult as OkNegotiatedContentResult<ContactRecord>;
            ContactRecord cRecord = response.Content;
            cRecord.CallEndDateTime = dateTime;
            cRecord.DispositionCode = dispositionValue;
            cRecord.CallState = "ORDER";
            cRecord.Notes = SessionManager.GetContactNotes(Session);
            CRC.PutContactRecord(cRecord);
        }

        public static void LoadCRMLead(HttpSessionStateBase Session, string actionXML)
        {
            string columnName = string.Empty;
            string columnValue = string.Empty;

            WorkFlowActionXMLObject wfXMLObject = DeserialezeWFActionXML(actionXML);
            string[] holdObjects = wfXMLObject.SetObj.Split('|');

            if (holdObjects.Length > 1)
                columnName = holdObjects[1]; 
            else
                columnName = "LeadRecordId";

            columnValue = SPutilities.ReplaceObjectsandQuestions(Session, wfXMLObject.SetValue, false);
            DataObjectLoader DOL = new DataObjectLoader();
            int loadobject = System.Convert.ToInt32(holdObjects[0]);
            DOL.LoadLeadObjectFromCRM(loadobject, columnName, columnValue, Session);
         
        }
        public static void DispositionNoSale(HttpSessionStateBase Session, string actionXML)
        {
            DateTime timeUtc = DateTime.UtcNow;
            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(ConfigurationManager.AppSettings["timeZone"]);
            DateTime dateTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, timeZone);

            WorkFlowActionXMLObject wfXMLObject = DeserialezeWFActionXML(actionXML);
            string dispositionValue = SPutilities.ReplaceObjectsandQuestions(Session, wfXMLObject.SetValue, false);

            int contactID = SessionManager.GetContactId(Session);
            ScreenViewer.API.ExternalData.ContactRecordController CRC = new ContactRecordController();
            var actionResult = CRC.GetContactRecordByContactId(contactID);

            var response = actionResult as OkNegotiatedContentResult<ContactRecord>;
            ContactRecord cRecord = response.Content;
            cRecord.CallEndDateTime = dateTime;
            cRecord.DispositionCode = dispositionValue;
            cRecord.CallState = "DISPO";
            cRecord.Notes = SessionManager.GetContactNotes(Session);
            CRC.PutContactRecord(cRecord);
        }

        public static WorkFlowActionXMLObject DeserialezeWFActionXML(string str)
        {
            XmlRootAttribute xRoot = new XmlRootAttribute();
            xRoot.ElementName = "ActionXML";
            xRoot.IsNullable = true;

            XmlSerializer XS = new XmlSerializer(typeof(WorkFlowActionXMLObject), xRoot);
            StringReader SR = new StringReader(str);
            return (WorkFlowActionXMLObject)XS.Deserialize(SR);
        }

        public static void SaveContactVariables(HttpSessionStateBase Session)
        {
            DateTime timeUtc = DateTime.UtcNow;
            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(ConfigurationManager.AppSettings["timeZone"]);
            DateTime dateTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, timeZone);

            VariableController VariableController = new API.VariableController();
            ScreenViewer.API.ProjectController PC = new API.ProjectController();
            var projectResult = PC.GetScriptProject(SessionManager.GetProjectId(Session));
            ScriptProject project = (ScriptProject)(projectResult as OkNegotiatedContentResult<ScriptProject>).Content;

            ScreenViewer.API.ExternalData.ContactRecordController CRC = new ContactRecordController();
            ContactVariable contactVariable = null;

            foreach (var item in project.ScriptProjectVariables)
            {
                contactVariable = new ContactVariable();
                contactVariable.ContactId = SessionManager.GetContactId(Session);
                contactVariable.VariableName = VariableController.GetVariableName(item.ScriptVariableID);
                contactVariable.VariableValue = SPutilities.GetVariableValue(Session, item.ScriptVariableID);
                contactVariable.CreatedDate = dateTime;

                if (!string.IsNullOrEmpty(contactVariable.VariableValue))
                    CRC.PostContactVariable(contactVariable);
            }
        }

        public static void SaveContactNavigation(HttpSessionStateBase Session)
        {
            DateTime timeUtc = DateTime.UtcNow;
            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(ConfigurationManager.AppSettings["timeZone"]);
            DateTime dateTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, timeZone);

            ScreenViewer.API.ExternalData.ContactRecordController CRC = new ContactRecordController();
            List<ContactNavigation> lstNavigation = SessionManager.GetNavigation(Session);

            if (lstNavigation != null)
            {
                int i = 1;
                foreach (ContactNavigation item in lstNavigation)
                {
                    item.Sequence = i++;
                    item.ContactId = SessionManager.GetContactId(Session);
                    item.ClientId = SessionManager.GetClientId(Session);
                    item.CreatedDate = dateTime;
                    CRC.PostContactNavigation(item);
                }
            }
        }

        public static bool RetrievePreviousCall(HttpSessionStateBase Session)
        {
            bool result = false;

            try
            {
                if (!string.IsNullOrEmpty(SessionManager.GetScriptParameterByKey("PreviousCallID", Session)))
                {
                    int contactID = Convert.ToInt32(SessionManager.GetScriptParameterByKey("PreviousCallID", Session));
                    ScreenViewer.API.ExternalData.ContactRecordController contactController = new API.ExternalData.ContactRecordController();
                    var actionResult = contactController.GetContactRecordByContactId(contactID);
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

                            SessionManager.AddUpdateQuestions(questvals, Session);

                            if (contactRecord.Orders != null && contactRecord.Orders.Count > 0)
                            {
                                List<ItemOrdered> iOrd = new List<ItemOrdered>();

                                SessionControl.SessionManager.StoreOrderId(Session, contactRecord.Orders.First().OrderId);

                                foreach (var item in contactRecord.Orders.First().OrderDetails)
                                {
                                    ItemOrdered io = new ItemOrdered();
                                    io.ItemCode = item.ItemCode;
                                    io.ItemQuantity = item.ItemQuantity;
                                    io.setKey = !string.IsNullOrEmpty(item.ItemKeys) ? true : false;
                                    iOrd.Add(io);
                                }


                                SessionManager.AddUpdateOrderedItems(iOrd, Session);
                            }
                        }

                        if (contactRecord.CallEndDateTime != null && contactRecord.CallEndDateTime != DateTime.MinValue)
                        {
                            TimeSpan span = contactRecord.CallEndDateTime.Value.Subtract(contactRecord.CallStartDateTime.Value);

                            if (span.Minutes > 5)
                            {
                                SessionManager.StoreScriptParameter("Critical", "Yes", Session);
                            }
                        }

                        if (!string.IsNullOrEmpty(contactRecord.LeadRecordId))
                        {
                            SessionManager.StoreProgramParameter("PreviousLeadRecordId", contactRecord.LeadRecordId, Session);
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

        private static decimal CreateOrder(int ContactID, HttpSessionStateBase Session)
        {
            DateTime timeUtc = DateTime.UtcNow;

            try
            {

                Order newOrder = new Order();
                newOrder.ContactId = ContactID;

                TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(ConfigurationManager.AppSettings["timeZone"]);
                DateTime dateTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, timeZone);

                newOrder.OrderDate = dateTime;

                if (string.IsNullOrEmpty(newOrder.OrderStatus))
                    newOrder.OrderStatus = "PLACED";

                newOrder.ModifiedDate = dateTime;
                newOrder.UserId = SessionManager.GetUserId(Session);
                newOrder.ClientId = SessionManager.GetClientId(Session);

                ScreenViewer.API.ExternalData.OrderController OC = new API.ExternalData.OrderController();
                var actionResult = OC.PostOrder(newOrder);

                Order theOrder;

                if (actionResult != null)
                {
                    var response = actionResult as OkNegotiatedContentResult<Order>;
                    theOrder = response.Content;
                    SessionControl.SessionManager.StoreOrderId(Session, theOrder.OrderId);
                }
                else
                {
                    return 0;
                }

                return theOrder.OrderId;
            }
            catch (Exception ex)
            {
                return 0;
            }

        }

        public static string BuildCallData(HttpSessionStateBase Session)
        {
            System.Text.StringBuilder data = new System.Text.StringBuilder();
            data.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            data.Append("<CallData>");

            List<QuestVal> questvals = SessionManager.GetQuestionResponses(Session);

            if (questvals != null)
            {
                data.Append("<ContactRecordDetail>");
                foreach (var item in questvals)
                {
                    data.Append("<Detail>");
                    data.Append(string.Format("<QuestionId>{0}</QuestionId>", item.QuestionID.ToString()));
                    data.Append(string.Format("<QuestionText>{0}</QuestionText>", item.QuestionText.ToString()));
                    data.Append(string.Format("<QuestionResponseText>{0}</QuestionResponseText>", item.DisplayResponse.ToString()));
                    data.Append(string.Format("<QuestionResponseValue>{0}</QuestionResponseValue>", item.Response.ToString()));
                    data.Append(string.Format("<QuestionKeys>{0}</QuestionKeys>", item.QuestKeyCodes));
                    data.Append("</Detail>");
                }
                data.Append("</ContactRecordDetail>");
            }

            List<ItemOrdered> collectionOrder = SessionManager.GetItemsOrdered(Session);

            if (collectionOrder != null)
            {
                data.Append("<OrderDetail>");
                foreach (var item in collectionOrder)
                {
                    data.Append("<Detail>");
                    data.Append(string.Format("<ItemCode>{0}</ItemCode>", item.ItemCode.ToString()));
                    data.Append(string.Format("<ItemName>{0}</ItemName>", item.ItemName.ToString()));
                    data.Append(string.Format("<ItemQuantity>{0}</ItemQuantity>", item.ItemQuantity.ToString()));
                    data.Append(string.Format("<ItemPrice>{0}</ItemPrice>", item.ItemPrice));
                    data.Append(string.Format("<ItemShipping>{0}</ItemShipping>", item.ItemShipping));
                    data.Append(string.Format("<ItemKeys>{0}</ItemKeys>", item.SetKeys));
                    data.Append("</Detail>");
                }
                data.Append("</OrderDetail>");
            }

            List<ContactNavigation> lstNavigation = SessionManager.GetNavigation(Session);

            if (lstNavigation != null)
            {
                data.Append("<NavigationDetail>");
                foreach (var item in lstNavigation)
                {
                    data.Append("<Detail>");
                    data.Append(string.Format("<Section>{0}</Section>", item.Section.ToString()));
                    data.Append(string.Format("<Workflow>{0}</Workflow>", item.WorkFlow.ToString()));
                    data.Append("</Detail>");
                }
                data.Append("</NavigationDetail>");
            }

            data.Append("</CallData>");
            return data.ToString();
        }

        private static void CreateMapping(LeadRecord leadRecord, dynamic holdLead)
        {

            if (IsPropertyExist(holdLead, "LeadOwner"))
                leadRecord.LeadOwner = holdLead.LeadOwner;

            if (IsPropertyExist(holdLead, "LeadStatus"))
                leadRecord.LeadStatus = holdLead.LeadStatus;

            if (IsPropertyExist(holdLead, "LeadSource"))
                leadRecord.LeadSource = holdLead.LeadSource;

            if (IsPropertyExist(holdLead, "LeadRating"))
                leadRecord.LeadRating = !string.IsNullOrEmpty(holdLead.LeadRating) ? Convert.ToDecimal(holdLead.LeadRating) : null;

            if (IsPropertyExist(holdLead, "LeadType"))
                leadRecord.LeadType = holdLead.LeadType;

            if (IsPropertyExist(holdLead, "Company"))
                leadRecord.Company = holdLead.Company;

            if (IsPropertyExist(holdLead, "Industry"))
                leadRecord.Industry = holdLead.Industry;

            if (IsPropertyExist(holdLead, "Title"))
                leadRecord.Title = holdLead.Title;

            if (IsPropertyExist(holdLead, "FirstName"))
                leadRecord.FirstName = holdLead.FirstName;

            if (IsPropertyExist(holdLead, "LastName"))
                leadRecord.LastName = holdLead.LastName;

            if (IsPropertyExist(holdLead, "MiddleName"))
                leadRecord.MiddleName = holdLead.MiddleName;

            if (IsPropertyExist(holdLead, "Email"))
                leadRecord.Email = holdLead.Email;

            if (IsPropertyExist(holdLead, "AltEmail"))
                leadRecord.AltEmail = holdLead.AltEmail;

            if (IsPropertyExist(holdLead, "Address1"))
                leadRecord.Address1 = holdLead.Address1;

            if (IsPropertyExist(holdLead, "Address2"))
                leadRecord.Address2 = holdLead.Address2;

            if (IsPropertyExist(holdLead, "State"))
                leadRecord.State = holdLead.State;

            if (IsPropertyExist(holdLead, "ZipCode"))
                leadRecord.ZipCode = holdLead.ZipCode;

            if (IsPropertyExist(holdLead, "Country"))
                leadRecord.Country = holdLead.Country;

            if (IsPropertyExist(holdLead, "Phone"))
                leadRecord.Phone = holdLead.Phone;

            if (IsPropertyExist(holdLead, "Mobile"))
                leadRecord.Mobile = holdLead.Mobile;

            if (IsPropertyExist(holdLead, "Gender"))
                leadRecord.Gender = holdLead.Gender;

            if (IsPropertyExist(holdLead, "PotRevenue"))
                leadRecord.PotRevenue = !string.IsNullOrEmpty(holdLead.PotRevenue) ? Convert.ToDecimal(holdLead.PotRevenue) : null;

            if (IsPropertyExist(holdLead, "InterestIn"))
                leadRecord.InterestIn = holdLead.InterestIn;

            if (IsPropertyExist(holdLead, "DNS"))
                leadRecord.DNS = (holdLead.DNS != "false");

            if (IsPropertyExist(holdLead, "DNC"))
                leadRecord.DNC = (holdLead.DNC != "false");

            if (IsPropertyExist(holdLead, "DNM"))
                leadRecord.DNM = (holdLead.DNM != "false");

            if (IsPropertyExist(holdLead, "TimeZone"))
                leadRecord.TimeZone = holdLead.TimeZone;

            foreach (LeadRecordAttribute attribute in leadRecord.LeadRecordAttributes)
            {
                if (IsPropertyExist(holdLead, attribute.LeadAttributeName))
                    attribute.LeadAttributeValue = holdLead.GetType().GetProperty(attribute.LeadAttributeName).GetValue(holdLead, null);
            }
        }
    }
}