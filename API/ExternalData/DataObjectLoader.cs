using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Net.Http;
using System.Net.Http.Headers;
using ScreenViewer.SessionControl;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Dynamic;
using System.Web.Http.Results;
using ScreenViewer.Models;
using ScreenViewer.Data;
using System.Text.RegularExpressions;

namespace ScreenViewer.API.ExternalData
{
    public class DataObjectLoader
    {

        public bool LoadDataObjectFromWebAPI(string DataObjectToLoad,string webAPIbaseaddress,string webAPIcall, HttpSessionStateBase SessionBase)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(webAPIbaseaddress);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = client.GetAsync(webAPIcall).Result;

            if (response.IsSuccessStatusCode)
            {
                var theresponse = response.Content.ReadAsStringAsync();
                string json = theresponse.Result.Replace("[", "").Replace("]", "");
                var converter = new ExpandoObjectConverter();
                var obj = JsonConvert.DeserializeObject(json);
                ScreenViewer.API.DataObjectController DOC = new DataObjectController();

                string DOname = DataObjectToLoad;

                var ObjectResult = DOC.GetDataObjectDictionary(DOname,obj);
                var LeadObject = ObjectResult as OkNegotiatedContentResult<DataObjects>;

                DataObjects DisplayObject = (DataObjects)LeadObject.Content;

                SessionManager.StoreDataObject(SessionBase, Regex.Split(DOname, "\\.")[0], DisplayObject);
                SessionManager.StoreProgramParameter(string.Format("{0}Found", DataObjectToLoad), "Yes", SessionBase);
                return false;
            }
            else
            {
                SessionManager.StoreProgramParameter(string.Format("{0}Found", DataObjectToLoad), "No", SessionBase);
            }

            return true;
        }

        public bool LoadLeadObjectFromCRM(int LoadObject, string columnName, string columnValue, HttpSessionStateBase SessionBase)
        {
            LeadRecordController LRC = new LeadRecordController();
            LeadRecord theLead;
            var actionresult = LRC.GetLeadRecordByColumn(columnName, columnValue);
            if (actionresult != null && actionresult != actionresult as System.Web.Http.Results.NotFoundResult)
            {
                var response = actionresult as OkNegotiatedContentResult<LeadRecord>;
                theLead = response.Content;
            }
            else
            {
                SessionManager.StoreProgramParameter("CRMLeadFound", "No", SessionBase);
                return false;
            }

            ScreenViewer.API.DataObjectController DOC = new DataObjectController();

            //string DOname = DataObjectToLoad;

            var ObjectResult = DOC.GetDataObjectCRM(LoadObject, theLead);
            var LeadObject = ObjectResult as OkNegotiatedContentResult<DataObjects>;

            DataObjects DisplayObject = (DataObjects)LeadObject.Content;

            SessionManager.StoreDataObject(SessionBase, DisplayObject.DOName, DisplayObject);
            SessionManager.StoreProgramParameter("CRMLeadFound", "Yes", SessionBase);
            SessionManager.StoreProgramParameter("CRMLeadObjectId", LoadObject.ToString(), SessionBase);

            return true;
        }

        public bool LoadDataObjectFromWebAPI(int scriptObjectId, string webAPIbaseaddress, HttpSessionStateBase SessionBase)
        {
            HttpClient client = new HttpClient();
            //client.BaseAddress = new Uri(webAPIbaseaddress);
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = client.GetAsync(webAPIbaseaddress).Result;

            if (response.IsSuccessStatusCode)
            {
                var theresponse = response.Content.ReadAsStringAsync();
                //string json = theresponse.Result.Replace("[", "").Replace("]", "");
                string json = theresponse.Result;

                //Example Multi Collection
                //string json = "[{\"Id\":\"00Qj000000IyrXzEAJ\",\"Company\":\"ZINGERS BAR AND GRILL\",\"Title\":null,\"Street\":\"12700 Hill Country Blvd\",\"City\":\"AUSTIN\",\"State\":\"Texas\",\"Country\":\"United States\",\"PostalCode\":\"78738\",\"Phone\":\"(512) 524-2430\",\"MobilePhone\":null,\"Email\":\"alan@zingersbarandgrill.com\",\"FirstName\":\"ALAN\",\"LastName\":\"MCARTHUR\",\"LeadSource\":\"List\",\"Status\":\"Open\",\"Lead_MID__c\":\"650000006747920\",\"Non_Sales_Disposition__c\":null,\"Products_Presented__c\":null,\"Card_Termination_Date__c\":\"2015-07-14\",\"Recent_or_Existing_Merchant_Acct__c\":null,\"Est_Annual_Process_Volume__c\":null,\"Average_Ticket__c\":null,\"Current_Processor__c\":null,\"Name_of_Existing_Equipment_softward__c\":null},{\"Id\":\"00Qj000000IyrXzEAJ\",\"Company\":\"ZINGERS BAR AND GRILL\",\"Title\":null,\"Street\":\"12700 Hill Country Blvd\",\"City\":\"AUSTIN\",\"State\":\"Texas\",\"Country\":\"United States\",\"PostalCode\":\"78738\",\"Phone\":\"(512) 524-2430\",\"MobilePhone\":null,\"Email\":\"alan@zingersbarandgrill.com\",\"FirstName\":\"ALAN\",\"LastName\":\"MCARTHUR\",\"LeadSource\":\"List\",\"Status\":\"Open\",\"Lead_MID__c\":\"650000006747920\",\"Non_Sales_Disposition__c\":null,\"Products_Presented__c\":null,\"Card_Termination_Date__c\":\"2015-07-14\",\"Recent_or_Existing_Merchant_Acct__c\":null,\"Est_Annual_Process_Volume__c\":null,\"Average_Ticket__c\":null,\"Current_Processor__c\":null,\"Name_of_Existing_Equipment_softward__c\":null}]";
                //Example Single Collection
                //string json = "[{\"Id\":\"00Qj000000IyrXzEAJ\",\"Company\":\"ZINGERS BAR AND GRILL\",\"Title\":null,\"Street\":\"12700 Hill Country Blvd\",\"City\":\"AUSTIN\",\"State\":\"Texas\",\"Country\":\"United States\",\"PostalCode\":\"78738\",\"Phone\":\"(512) 524-2430\",\"MobilePhone\":null,\"Email\":\"alan@zingersbarandgrill.com\",\"FirstName\":\"ALAN\",\"LastName\":\"MCARTHUR\",\"LeadSource\":\"List\",\"Status\":\"Open\",\"Lead_MID__c\":\"650000006747920\",\"Non_Sales_Disposition__c\":null,\"Products_Presented__c\":null,\"Card_Termination_Date__c\":\"2015-07-14\",\"Recent_or_Existing_Merchant_Acct__c\":null,\"Est_Annual_Process_Volume__c\":null,\"Average_Ticket__c\":null,\"Current_Processor__c\":null,\"Name_of_Existing_Equipment_softward__c\":null}]";
                //Example No Collection
                //string json = "{\"Id\":\"00Qj000000IyrXzEAJ\",\"Company\":\"ZINGERS BAR AND GRILL\",\"Title\":null,\"Street\":\"12700 Hill Country Blvd\",\"City\":\"AUSTIN\",\"State\":\"Texas\",\"Country\":\"United States\",\"PostalCode\":\"78738\",\"Phone\":\"(512) 524-2430\",\"MobilePhone\":null,\"Email\":\"alan@zingersbarandgrill.com\",\"FirstName\":\"ALAN\",\"LastName\":\"MCARTHUR\",\"LeadSource\":\"List\",\"Status\":\"Open\",\"Lead_MID__c\":\"650000006747920\",\"Non_Sales_Disposition__c\":null,\"Products_Presented__c\":null,\"Card_Termination_Date__c\":\"2015-07-14\",\"Recent_or_Existing_Merchant_Acct__c\":null,\"Est_Annual_Process_Volume__c\":null,\"Average_Ticket__c\":null,\"Current_Processor__c\":null,\"Name_of_Existing_Equipment_softward__c\":null}";
                //Example Collection for DropDown
                //string json = "[{\"Name\":\"Do Not Call\",\"Value\":\"Do_Not_Call\"},{\"Name\":\"Call Back\",\"Value\":\"Call_Back\"},{\"Name\":\"Fax Machine\",\"Value\":\"Fax_Machine\"},{\"Name\":\"Wrong Number\",\"Value\":\"Wrong_Number\"}]";

                var converter = new ExpandoObjectConverter();

                var obj = JsonConvert.DeserializeObject(json);

                //string json = "[{\"Dispo\":{\"Name\":\"Do Not Call\",\"Value\":\"Do_Not_Call\"}},{\"Dispo\":{\"Name\":\"Call Back\",\"Value\":\"Call_Back\"}},{\"Dispo\":{\"Name\":\"Fax Machine\",\"Value\":\"Fax_Machine\"}},{\"Dispo\":{\"Name\":\"Wrong Number\",\"Value\":\"Wrong_Number\"}}]";
                //var obj = JsonConvert.DeserializeObject<List<Dictionary<string,Dictionary<string, string>>>>(json);

                ScreenViewer.API.DataObjectController DOC = new DataObjectController();

                var ObjectResult = DOC.GetDataObjectDictionary(scriptObjectId, obj);
                var LeadObject = ObjectResult as OkNegotiatedContentResult<DataObjects>;

                DataObjects DisplayObject = (DataObjects)LeadObject.Content;

                SessionManager.StoreDataObject(SessionBase, DisplayObject.DOName, DisplayObject);
                SessionManager.StoreProgramParameter(string.Format("{0}Found", DisplayObject.DOName), "Yes", SessionBase);
                return true;
            }

            return false;
        }

    }
}