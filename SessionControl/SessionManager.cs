using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ScreenViewer.Models;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using System.Web.Http.Results;
using Kendo.Mvc.UI;

namespace ScreenViewer.SessionControl
{
    public static class SessionManager
    {
        const string prefix = "DO_";

        public static bool StoreDataObject(HttpSessionStateBase Session,String DataObjectName,Object TheDataObject)
        {
            string doname = prefix + DataObjectName;
            Session[doname] = TheDataObject;
            return true;
        }

        public static bool StoreObject(HttpSessionStateBase Session, String objectName, Object theObject)
        {
            Session[objectName] = theObject;
            return true;
        }

        public static bool UpdateDataObjectValue(HttpSessionStateBase Session,String DataObjectName, string TheValue)
        {
            return true;
        }

        public static void ClearSessionData(HttpSessionStateBase Session)
        {
            string returnURL = Session["ReturnURL"] != null ? Session["ReturnURL"].ToString() : string.Empty;
            Session.Clear();

            if (!string.IsNullOrEmpty(returnURL))
                Session["ReturnURL"] = returnURL;

        }

        public static Object GetDataObject(HttpSessionStateBase Session, string DataObjectName)
        {
            string doname = prefix + DataObjectName;
            Object ReturnObject = Session[doname] == null ? null : Session[doname];
            return ReturnObject;
        }

        public static Object GetObject(HttpSessionStateBase Session, string objectName)
        {
            Object returnObject = Session[objectName] == null ? null : Session[objectName];
            return returnObject;
        }

        public static void StoreContactId(HttpSessionStateBase Session, int contactId)
        {
            Session["ContactId"] = contactId;
        }

        public static void StoreContactNotes(HttpSessionStateBase Session, string theNotes)
        {
            Session["ContactNotes"] = theNotes;
        }

        public static string GetContactNotes(HttpSessionStateBase Session)
        {
            string thentes = (Session["ContactNotes"] == null) ? "" : Session["ContactNotes"].ToString(); 
            return thentes;
        }

        public static void StoreNextNode(HttpSessionStateBase Session, WFNodeInfo node)
        {
            Session["NextNode"] = node;
        }

        public static WFNodeInfo GetNextNode(HttpSessionStateBase Session)
        {
            WFNodeInfo node = (Session["NextNode"] == null) ? null : (WFNodeInfo)Session["NextNode"];
            return node;
        }

        public static void StoreOrderId(HttpSessionStateBase Session, decimal OrderId)
        {
            Session["OrderId"] = OrderId;
        }

        public static void StoreProjectName(HttpSessionStateBase Session, string projectName)
        {
            Session["ProjectName"] = projectName;
        }

        public static void StoreProjectDescription(HttpSessionStateBase Session, string projectDescription)
        {
            Session["ProjectDescription"] = projectDescription;
        }

        public static void StoreScreenLayout(HttpSessionStateBase Session, string screenLayout)
        {
            Session["ScreenLayout"] = screenLayout;
        }

        public static void StoreNotificationText(HttpSessionStateBase Session, string notification)
        {
            Session["NotificationText"] = notification;
        }

        public static void StoreLayoutHTML(HttpSessionStateBase Session, string layout)
        {
            Session["LayoutHTML"] = layout;
        }

        public static string GetLayoutHTML(HttpSessionStateBase Session)
        {
            return Session["LayoutHTML"] != null ? (string)Session["LayoutHTML"] : null;
        }

        public static void StoreProjectId(HttpSessionStateBase Session, int projectId)
        {
            Session["ProjectId"] = projectId;
        }

        public static void StoreLogoImage(HttpSessionStateBase Session, byte[] imageBytes)
        {
            Session["LogoImage"] = imageBytes;
        }

        public static void StoreDisposition(HttpSessionStateBase Session, string disposition)
        {
            Session["Disposition"] = disposition;
        }

        public static void StoreMenu(HttpSessionStateBase Session, List<MenuItem> lstMenu)
        {
            Session["Menu"] = lstMenu;
        }
        public static void StoreMenu(HttpSessionStateBase Session, string theMenu)
        {
            Session["Menu"] = theMenu;
        }

        public static void ClearMenu(HttpSessionStateBase Session)
        {
            Session["Menu"] = null;
        }
        public static int GetContactId(HttpSessionStateBase Session)
        {
            return Convert.ToInt32(Session["ContactId"]);
        }

        public static decimal GetOrderId(HttpSessionStateBase Session)
        {
            return Convert.ToDecimal(Session["OrderId"]);
        }

        public static byte[] GetLogoImage(HttpSessionStateBase Session)
        {
            return Session["LogoImage"] != null ? (byte[])Session["LogoImage"] : null;
        }

        public static string GetProjectName(HttpSessionStateBase Session)
        {
            return Session["ProjectName"] != null ? Session["ProjectName"].ToString() : string.Empty;
        }

        public static string GetScreenLayout(HttpSessionStateBase Session)
        {
            return Session["ScreenLayout"] != null ? Session["ScreenLayout"].ToString() : string.Empty;
        }

        public static int GetProjectId(HttpSessionStateBase Session)
        {
            return Session["ProjectId"] != null ? (int)Session["ProjectId"] : 0;
        }

        public static string GetClientId(HttpSessionStateBase Session)
        {
            string clientId = string.Empty;

            //if (Session["ClientId"] != null && !string.IsNullOrEmpty(Session["ClientId"].ToString()))
            //    clientId = Session["ClientId"].ToString();
            //else
            //    clientId = GetScriptParameterByKey("ClientId", Session) != null ? GetScriptParameterByKey("ClientId", Session) : string.Empty;

            //return clientId;

            HttpCookie myCookie = System.Web.HttpContext.Current.Request.Cookies["myCookie"];

            if (!string.IsNullOrEmpty(myCookie.Values["ClientId"]))
            {
                clientId = myCookie.Values["ClientId"].ToString();
            }

            return clientId;
        }

        public static string GetUserId(HttpSessionStateBase Session)
        {
            string userId = string.Empty;

            //if (Session["UserId"] != null && !string.IsNullOrEmpty(Session["UserId"].ToString()))
            //    return Session["UserId"].ToString();
            //else
            //    return string.Empty;

            HttpCookie myCookie = System.Web.HttpContext.Current.Request.Cookies["myCookie"];

            if (!string.IsNullOrEmpty(myCookie.Values["UserId"]))
            {
                userId = myCookie.Values["UserId"].ToString();
            }

            return userId;
        }

        public static string GetAgentName(HttpSessionStateBase Session)
        {
            string agentName = string.Empty;

            HttpCookie myCookie = System.Web.HttpContext.Current.Request.Cookies["myCookie"];

            if (!string.IsNullOrEmpty(myCookie.Values["AgentName"]))
            {
                agentName = myCookie.Values["AgentName"].ToString();
            }

            return agentName;
        }

        public static string GetDisposition(HttpSessionStateBase Session)
        {
            return Session["Disposition"] != null ? Session["Disposition"].ToString() : string.Empty;
        }

        public static IEnumerable<MenuItem> GetMenu(HttpSessionStateBase Session)
        {
            return Session["Menu"] != null ? (IEnumerable<MenuItem>)Session["Menu"] : null;
        }

        public static string GetMenuHTML(HttpSessionStateBase Session)
        {
            return Session["Menu"] != null ? (string)Session["Menu"] : null;
        }

        public static string GetNotificationText(HttpSessionStateBase Session)
        {
            return Session["NotificationText"] != null ? (string)Session["NotificationText"] : null;
        }

        public static bool StoreParameters(HttpSessionStateBase Session,Dictionary<string,string> Params)
        {
            Session["ProgramParametes"] = Params;
            return true;
        }

        public static string ReturnParameter(HttpSessionStateBase Session, string ParameterName)
        {
            
            if (ParameterName.StartsWith("Order."))
            {
                return getOrderParameters(Session, ParameterName);
            }            
            if (ParameterName.StartsWith("Tax."))
            {
                return getTaxParameters(Session, ParameterName);
            }
            
            Dictionary<string, string> ParamDict;
            var returnDict = ReturnParameters(Session);
            if (returnDict == null)
            {
                return null;
            }
            ParamDict = (Dictionary<string, string>)returnDict;
            string keyvalue;
            if (ParamDict.TryGetValue(ParameterName,out keyvalue))
            {
                return keyvalue;
            }
            else
            {
                return "";
            }
        }
        public static string getOrderParameters(HttpSessionStateBase Session, string ParameterName)
        {
            List<ItemOrdered> itemsordered = GetItemsOrdered(Session);
            decimal taxrate = Convert.ToDecimal(ReturnParameter(Session,"Tax.TaxRate"));
            bool taxshipping = Convert.ToBoolean(ReturnParameter(Session,"Tax.TaxShipping"));

            decimal itemtotal = 0;
            decimal shiptotal = 0;
            decimal taxtotal = 0;

            if (itemsordered == null) return "0";

            foreach (ItemOrdered item in itemsordered)
            {

                itemtotal += (item.ItemPrice * item.ItemQuantity);
                shiptotal += (item.ItemShipping * item.ItemQuantity);
            }
            if (taxshipping)
            {
                taxtotal = (itemtotal + shiptotal) * taxrate;
            }
            else
            {
                taxtotal = itemtotal * taxrate;
            }


            switch (ParameterName)
            {
                case "Order.OrderTotal":
                    return (itemtotal + shiptotal + taxtotal).ToString("F");
                case "Order.ShippingTotal":
                    return shiptotal.ToString("F");
                case "Order.TaxTotal" :
                    return taxtotal.ToString("F");
                default:
                    return"";
            }
        }

        public static string getTaxParameters(HttpSessionStateBase Session, string ParameterName)
        {
           

            switch (ParameterName)
            {
                case "Tax.TaxRate":
                   decimal taxrate = 0;

                   try
                   {
                       taxrate = Convert.ToDecimal(SessionControl.SessionManager.ReturnParameter(Session, "TaxRate"));
                       if (taxrate > 0) taxrate = taxrate / 100;
                   }
                   catch
                   {
                       taxrate = 0;
                   }
                   return taxrate.ToString();

                case "Tax.TaxShipping":
                   bool taxshipping = true;
                   try
                   {
                       taxshipping = Convert.ToBoolean(SessionControl.SessionManager.ReturnParameter(Session, "TaxShipping"));
                       return taxshipping.ToString();
                   }
                   catch
                   {
                       return true.ToString();
                   }
                default:
                   return "";
            }



        }
        public static Dictionary<string,string> ReturnParameters(HttpSessionStateBase Session)
        {
            var ParamList = Session["ProgramParametes"];
            if (ParamList == null)
            {
                return null;
            }

            return (Dictionary<string, string>)ParamList;
        }

        public static void StoreWorkflowHistory(HttpSessionStateBase Session, wfHistory[] history)
        {
            Session["workflowhistory"] = history;
        }
        public static void ClearWorkflowHistory(HttpSessionStateBase Session)
        {
            Session["workflowhistory"] = null;
        }

        public static wfHistory[] GetWorkflowHistory(HttpSessionStateBase Session)
        {
            object viewState = Session["workflowhistory"];

            if (viewState == null)
            {
                wfHistory[] arrayList = new wfHistory[1];
                return arrayList;
            }
            else
            {
                return (wfHistory[])viewState;
            }
        }

        public static void StoreNavigation(HttpSessionStateBase Session, string section, string workflow)
        {
            Data.ContactNavigation navigation = new Data.ContactNavigation();

            object viewState = Session["Navigation"];

            if (viewState != null)
            {
                List<Data.ContactNavigation> listNavigation  = (List<Data.ContactNavigation>)viewState;
                listNavigation.Add(new Data.ContactNavigation { Section = section, WorkFlow = workflow, CreatedDate = DateTime.Now });
                Session["Navigation"] = listNavigation;
            }
            else
            {
                List<Data.ContactNavigation> listNavigation = new List<Data.ContactNavigation>();
                listNavigation.Add(new Data.ContactNavigation { Section = section, WorkFlow = workflow, CreatedDate = DateTime.Now });
                Session["Navigation"] = listNavigation;
            }
        }

        public static List<Data.ContactNavigation> GetNavigation(HttpSessionStateBase Session)
        {
            object viewState = Session["Navigation"];

             if (viewState != null)
             {
                 return (List<Data.ContactNavigation>)viewState;
             }
             else
                 return null;
        }

        public static void StoreScriptParameter(Dictionary<string, string> parametes, HttpSessionStateBase Session)
        {
            Session["ScriptParametes"] = parametes;
        }

        public static Dictionary<string, string> GetScriptParameter(HttpSessionStateBase Session)
        {
            object viewState = Session["ScriptParametes"];

            if (viewState != null)
            {
                return (Dictionary<string, string>)viewState;
            }
            else
                return null;
        }

        public static void StoreProgramParameter(Dictionary<string, string> parametes, HttpSessionStateBase Session)
        {
            Session["ProgramParametes"] = parametes;
        }

        public static Dictionary<string, string> GetProgramParameter(HttpSessionStateBase Session)
        {
             object viewState = Session["ProgramParametes"];

             if (viewState != null)
             {
                 return (Dictionary<string, string>)viewState;
             }
             else
                 return null;
        }

        public static string GetProgramParameterByKey(string key, HttpSessionStateBase Session)
        {
            if (key.StartsWith("Order."))
            {
                return getOrderParameters(Session, key);
            }
            if (key.StartsWith("Tax."))
            {
                return getTaxParameters(Session, key);
            }
            
            
            
            string returnValue = string.Empty;
            object viewState = Session["ProgramParametes"];

            if (viewState != null)
            {
                Dictionary<string, string> parameters =  (Dictionary<string, string>)viewState;
                if (parameters.ContainsKey(key))
                {
                    returnValue =  parameters[key];
                }
            }

            return returnValue;
        }

        public static string GetScriptParameterByKey(string key, HttpSessionStateBase Session)
        {



            string returnValue = string.Empty;
            object viewState = Session["ScriptParametes"];

            if (viewState != null)
            {
                Dictionary<string, string> parameters = (Dictionary<string, string>)viewState;
                if (parameters.ContainsKey(key))
                {
                    returnValue = parameters[key];
                }
            }

            return returnValue;
        }
        public static bool StoreProgramParameter(string param, string value, HttpSessionStateBase Session)
        {
            string returnValue = string.Empty;
            object viewState = Session["ProgramParametes"];

            if (viewState != null)
            {
                Dictionary<string, string> parameters = (Dictionary<string, string>)viewState;
                if (parameters.ContainsKey(param))
                {
                    parameters[param] = value;
                }
                else
                {
                    parameters.Add(param, value);
                }
                Session["ProgramParametes"] = parameters;
            }
            else
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add(param, value);
                Session["ProgramParametes"] = parameters;
            }


            return true;
        }

        public static bool StoreScriptParameter(string param, string value, HttpSessionStateBase Session)
        {
            string returnValue = string.Empty;
            object viewState = Session["ScriptParametes"];

            if (viewState != null)
            {
                Dictionary<string, string> parameters = (Dictionary<string, string>)viewState;
                if (parameters.ContainsKey(param))
                {
                    parameters[param] = value;
                }
                else
                {
                    parameters.Add(param, value);
                }
                Session["ScriptParametes"] = parameters;
            }
            else
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add(param, value);
                Session["ScriptParametes"] = parameters;
            }


            return true;
        }


        public static List<QuestVal> GetQuestionResponses(HttpSessionStateBase Session)
        {
            return (List<QuestVal>)Session["QuestionResponses"];
        }

        public static string GetQuestionResponse(string QuestionID, HttpSessionStateBase Session)
        {
            List<QuestVal> QuestionValues = (List<QuestVal>)Session["QuestionResponses"];

            if (QuestionValues == null) return null;

            foreach (QuestVal qresp in QuestionValues)
            {
                if (qresp.QuestionID == QuestionID)
                {
                    return qresp.Response;
                }
            }

            return null;
        }

        public static string GetQuestionDisplayResponse(string QuestionID, HttpSessionStateBase Session)
        {
            List<QuestVal> QuestionValues = (List<QuestVal>)Session["QuestionResponses"];

            if (QuestionValues == null) return null;

            foreach (QuestVal qresp in QuestionValues)
            {
                if (qresp.QuestionID == QuestionID)
                {
                    return qresp.DisplayResponse;
                }
            }

            return null;
        }
        public static string GetResponsebyKeycode(string keycode,HttpSessionStateBase Session)
        {
            List<QuestVal> QuestionValues = (List<QuestVal>)Session["QuestionResponses"];
            if (QuestionValues == null) return null;

            foreach (QuestVal qresp in QuestionValues)
            {
                string[] thecodes = Regex.Split(qresp.QuestKeyCodes, @"\|\|");
                int inlist = Array.IndexOf(thecodes, keycode);

                if (inlist >= 0)
                {
                    return qresp.Response;
                }
            }

            return null;
        }
        public static string GetDisplayResponsebyKeycode(string keycode, HttpSessionStateBase Session)
        {
            List<QuestVal> QuestionValues = (List<QuestVal>)Session["QuestionResponses"];
            if (QuestionValues == null) return null;

            foreach (QuestVal qresp in QuestionValues)
            {
                string[] thecodes = Regex.Split(qresp.QuestKeyCodes, @"\|\|");
                int inlist = Array.IndexOf(thecodes, keycode);

                if (inlist >= 0)
                {
                    return qresp.DisplayResponse;
                }
            }

            return null;
        }
        public static string[] GetResponsesbyKeycode(string keycode, HttpSessionStateBase Session)
        {
            StringCollection keyresponses = new StringCollection();

            List<QuestVal> QuestionValues = (List<QuestVal>)Session["QuestionResponses"];

            if (QuestionValues == null) return null;

            foreach (QuestVal qresp in QuestionValues)
            {
                string[] thecodes = Regex.Split(qresp.QuestKeyCodes, @"\|\|");
                int inlist = Array.IndexOf(thecodes, keycode);

                if (inlist > 0)
                {
                    keyresponses.Add(qresp.Response);
                }
            }

            if (keyresponses.Count > 0)
            {
                string[] returnarray = new string[keyresponses.Count - 1];
                keyresponses.CopyTo(returnarray, 0);
                return returnarray;
            }

            return null;
        }

        public static void AddUpdateAQuestion(QuestVal QuestionValue, HttpSessionStateBase Session)
        {
            AddUpdateQuestion(QuestionValue.QuestionID, QuestionValue.Response, QuestionValue.DisplayResponse ,Session);
        }

        public static void AddUpdateQuestions(List<QuestVal> QuestionValues, HttpSessionStateBase Session)
        {
            foreach (QuestVal QV in QuestionValues)
            {
                AddUpdateQuestion(QV.QuestionID, QV.Response, QV.DisplayResponse, Session);
            }
        }

        public static bool AddUpdateQuestion(string[] qid, string responsevalue, string responsetext, HttpSessionStateBase Session)
        {
            string thevalue = string.Empty;

            if (responsevalue.Contains("|"))
            {
                string[] responses = responsevalue.Split('|');
                for (int i= 0; i < qid.Length; i++)
                {
                    thevalue = SPutilities.ReplaceObjectsandQuestions(Session, responses[i], false);
                    if (thevalue.Contains("Value was not found")) thevalue = string.Empty;
                    AddUpdateQuestion(qid[i], thevalue, thevalue, Session);
                }
            }
            else
            {
                thevalue = SPutilities.ReplaceObjectsandQuestions(Session, responsevalue, false);
                foreach (string q in qid)
                {
                    AddUpdateQuestion(q, responsevalue, responsetext, Session);
                }
            }

            return true;
        }

        public static  bool AddUpdateQuestion(string qid, string responsevalue, string responsetext, HttpSessionStateBase Session)
        {
            if (string.IsNullOrEmpty(responsetext))
            {
                responsetext = responsevalue;
            }
            List<QuestVal> QuestionValues = (List<QuestVal>)Session["QuestionResponses"];
            
            int foundindex = -1;
            int currentcount = 0;

            if (QuestionValues != null)
            {
                foreach (QuestVal qresp in QuestionValues)
                {
                    if (qid == qresp.QuestionID)
                    {
                        foundindex = currentcount;
                        break;
                    }
                    currentcount++;
                }
            }
            else
            {
                QuestionValues = new List<QuestVal>();
            }
            if (foundindex >= 0)
            {
                QuestVal QVM = QuestionValues[foundindex];
                QVM.Response = responsevalue;
                QVM.DisplayResponse = responsetext;
                QuestionValues.RemoveAt(foundindex);
                QuestionValues.Add(QVM);
                Session["QuestionResponses"] = QuestionValues;

                return true;
            }

            try
            {
                ScreenViewer.API.Elements.QuestionController QC = new API.Elements.QuestionController();
                var actionResult = QC.GetScriptQuestion(System.Convert.ToDecimal(qid));
                var response = actionResult as OkNegotiatedContentResult<Data.ScriptQuestion>;
                Data.ScriptQuestion SQ = response.Content;
                QuestVal newquest = new QuestVal();
                newquest.QuestionID = SQ.ScriptQuestionID.ToString();
                newquest.QuestionText = SQ.QuestionText;
                newquest.QuestKeyCodes = SQ.KeyCodes;
                newquest.Response = responsevalue;
                newquest.DisplayResponse = responsetext;
                QuestionValues.Add(newquest);
                Session["QuestionResponses"] = QuestionValues;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static List<ItemOrdered> GetItemsOrdered(HttpSessionStateBase Session)
        {
            return (List<ItemOrdered>)Session["ItemsOrdered"];
        }
        public static void ClearOrderCart(HttpSessionStateBase Session)
        {
            Session["ItemsOrdered"] = null;
        }
        public static void AddUpdateOrderedItems(List<ItemOrdered> OIValues, HttpSessionStateBase Session)
        {
            foreach (ItemOrdered IO in OIValues)
            {
                AddUpdateOrderItem(IO, Session);
            }
        }

        public static bool AddUpdateOrderItem(ItemOrdered Item, HttpSessionStateBase Session)
        {
            List<ItemOrdered> Items = (List<ItemOrdered>)Session["ItemsOrdered"];

            int foundindex = -1;
            int currentcount = 0;

            if (Items != null)
            {
                foreach (ItemOrdered IO2 in Items)
                {
                    if (Item.ItemCode == IO2.ItemCode)
                    {
                        foundindex = currentcount;
                        break;
                    }
                    currentcount++;
                }
            }
            else
            {
                Items = new List<ItemOrdered>();
            }

            if (foundindex >= 0)
            {
                ItemOrdered IOS = Items[foundindex];
                IOS.ItemQuantity = Item.ItemQuantity;

                Items.RemoveAt(foundindex);

                if (IOS.ItemQuantity > 0)
                {
                    Items.Add(IOS);

                    if (IOS.setKey)
                    {
                        List<string> KeyList = Regex.Split(IOS.SetKeys, ",").ToList();
                        SetItemKeys(Session, KeyList, "I");
                    }
                }
                StoreItemsOrdered(Session, Items);

                //Session["ItemsOrdered"] = Items;

                return true;
            }

            try
            {
                if (Item.ItemQuantity == 0)
                {
                    return true;
                }

                ScreenViewer.API.ItemController IC = new API.ItemController();
                var actionResult = IC.GetScriptItemByCode(Item.ItemCode);
                var response = actionResult as OkNegotiatedContentResult<Data.ScriptItem>;
                Data.ScriptItem SI = response.Content;
                
                ItemOrdered newitem = new ItemOrdered();

                newitem.ItemCode = SI.ItemCode;
                newitem.Category = SI.ItemCategory;
                newitem.SubCategory = SI.ItemSubCategory;
                newitem.ItemDesc = SI.ItemDesc;
                newitem.ItemName = SI.ItemName;
                newitem.ItemPrice =   (decimal)SI.ItemPrice;
                newitem.ItemQuantity = Item.ItemQuantity;
                newitem.ItemShipping = (decimal)SI.ItemShippingPrice;
                newitem.setKey = Item.setKey;
                //newitem.SetKeys = SI.ItemSetKeys; ??
                newitem.oiOwner = Item.oiOwner;
                Items.Add(newitem);

                StoreItemsOrdered(Session, Items);
                return true;
            }
            catch
            {
                return false;
            }
        }


        public static void StoreItemsOrdered(HttpSessionStateBase Session, List<ItemOrdered> theItems)
        {
            Session["ItemsOrdered"] = theItems;

        }

        public static bool StoreItemKeys(Dictionary<string,string> ItemKeys, HttpSessionStateBase Session)
        {
            Session["ItemKeys"] = ItemKeys;
            return true;
        }

        public static Dictionary<string, string> ReturnItemKeys( HttpSessionStateBase Session)
        {
            return (Dictionary<string, string>)Session["ItemKeys"];
        }

        public static bool SetItemKeys(HttpSessionStateBase Session, List<string> theKeys, string from)
        {

            Dictionary<string, string> ItemDict = ReturnItemKeys(Session);

            if (ItemDict == null) ItemDict = new Dictionary<string, string>();

            foreach (string key in theKeys)
            {
                string ItemKey = Regex.Split(key, ":")[0];
                string ItemKeyValue = Regex.Split(key, ":")[1];


                if (from == "A")
                {
                    if (ItemDict.ContainsKey("A:" + ItemKey))
                    {
                        ItemDict["A:" + ItemKey] = ItemKeyValue;
                    }
                    else
                    {
                        if (ItemDict.ContainsKey("I:" + ItemKey))
                        {
                            ItemDict.Add("A:" + ItemKey, ItemKeyValue);
                            ItemDict.Remove("I:" + ItemKey);
                        }
                        else
                        {
                            ItemDict.Add("A:" + ItemKey, ItemKeyValue);
                        }
                    }
                }
                else
                {

                    if (ItemDict.ContainsKey("I:" + ItemKey))
                    {
                        ItemDict["I:" + ItemKey] = ItemKeyValue;
                    }
                    else
                    {
                        if (ItemDict.ContainsKey("A:" + ItemKey))
                        {
                        }
                        else
                        {
                            ItemDict.Add("I:" + ItemKey, ItemKeyValue);
                        }
                    }

                }

            }

            StoreItemKeys(ItemDict, Session);
            return true;

        }


        public static bool SetItemKey(HttpSessionStateBase Session,string ItemKey, string ItemKeyValue,string from)
        {

            Dictionary<string, string> ItemDict = ReturnItemKeys(Session);

            if (ItemDict == null)
            {
                ItemDict = new Dictionary<string, string>();
            }
            if (from == "A")
            {
                if (ItemDict.ContainsKey("A:" + ItemKey))
                {
                    ItemDict["A:" + ItemKey] = ItemKeyValue;
                }
                else
                {
                    if (ItemDict.ContainsKey("I:" + ItemKey))
                    {
                        ItemDict.Add("A:" + ItemKey, ItemKeyValue);
                        ItemDict.Remove("I:" + ItemKey);
                    }
                    else
                    {
                        ItemDict.Add("A:" + ItemKey, ItemKeyValue);
                    }
                }
            }
            else
            {

                if (ItemDict.ContainsKey("I:" + ItemKey))
                {
                    ItemDict["I:" + ItemKey] = ItemKeyValue;
                }
                else
                {
                    if (ItemDict.ContainsKey("A:" + ItemKey))
                    {
                    }
                    else
                    {
                        ItemDict.Add("I:" + ItemKey, ItemKeyValue);
                    }
                }

            }

            StoreItemKeys(ItemDict, Session);
            return true;

        }
        public static bool RemoveItemKey(HttpSessionStateBase Session, string ItemKey, string from)
        {

            Dictionary<string, string> ItemDict = ReturnItemKeys(Session);
            if (ItemDict == null)
            {
                return true;
            }

            if (from == "A")
            {
                if (ItemDict.ContainsKey("A:" + ItemKey))
                {
                    ItemDict.Remove("A:" + ItemKey);
                }
                else
                {
                    if (ItemDict.ContainsKey("I:" + ItemKey))
                    {
                        ItemDict.Remove("A:" + ItemKey);
                    }

                }
            }
            else
            {
                if (ItemDict.ContainsKey("I:" + ItemKey))
                {
                    ItemDict.Remove("I:" + ItemKey);
                }
            }

            StoreItemKeys(ItemDict, Session);
            return true;

        }


        public static bool syncKeys(HttpSessionStateBase Session)
        {
            Dictionary<string, string> ItemDict = ReturnItemKeys(Session);
            if (ItemDict == null)
            {
                return true;
            }
 
            
            List<ItemOrdered> Items = (List<ItemOrdered>)Session["ItemsOrdered"];


            foreach (string itemkeyplus in ItemDict.Keys)
            {
                if (itemkeyplus.StartsWith("I:"))
            {
                bool foundkeyset = false;
                string newkeyval = "";
                    string itemkey = itemkeyplus.Remove(0, 2);
                foreach (ItemOrdered item in Items)
                {
                    if (item.SetKeys != "")
                    {
                        List<string> Keystrs = Regex.Split(item.SetKeys, ",").ToList();
                        foreach (string keystr in Keystrs)
                        {
                            string thekey = Regex.Split(keystr, ":")[0];
                            if (thekey == itemkey)
                            {
                                foundkeyset = true;
                                newkeyval = Regex.Split(keystr, ":")[1];
                                break;
                            }
                        }
                        if (!foundkeyset)
                        {
                            ItemDict.Remove(itemkey);
                        }
                        else
                        {
                                ItemDict["I:" + itemkey] = newkeyval;

                        }

                    }
                }
                }
            }
            StoreItemKeys(ItemDict, Session);
            return true;
        }

        public static void Remove(HttpSessionStateBase Session, string name)
        {
            Session[name] = null;
        }

        public static void AddScriptURL(string url, HttpSessionStateBase Session)
        {
            Session["ScriptURL"] = url;
        }

        public static string GetScriptURL(HttpSessionStateBase Session)
        {
            if (Session["ScriptURL"] != null)
                return Session["ScriptURL"].ToString();
            else
                return string.Empty;
        }

    }
}