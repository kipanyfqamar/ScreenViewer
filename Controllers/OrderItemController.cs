using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using ScreenViewer;
using ScreenViewer.API.Elements;
using System.Web.Http.Results;
using System.Text.RegularExpressions;
using ScreenViewer.Models;
using ScreenViewer.Data;

namespace ScreenViewer.Controllers
{
    public class OrderItemController : Controller
    {
        private ScreenPlayEntities db = new ScreenPlayEntities();

        public string Render(int? id, bool criticalelement,ControllerContext ContCont)
        {
            API.Elements.OrderItemController OC = new API.Elements.OrderItemController();
            var actionResult = OC.GetScriptOrderItem((int)id);

            if (actionResult != null && actionResult != actionResult as System.Web.Http.Results.NotFoundResult)
            {
                ScriptItemSelector scriptorderitem = (actionResult as OkNegotiatedContentResult<ScriptItemSelector>).Content;

                //scriptorderitem.DisplayColumns += string.Format(",{0},{1},{2},{3}", "quantity", "filterKey", "ItemMaxQuantity", "Code");

                if (criticalelement)
                {
                    ViewBag.OID = "C_" + scriptorderitem.ScriptItemSelectorID.ToString();
                    ViewBag.Critical = "Y";
                }
                else
                {
                    ViewBag.OID = "N_" + scriptorderitem.ScriptItemSelectorID.ToString();
                    ViewBag.Critical = "N";

                }

                if (scriptorderitem.LayoutType == "F")
                    scriptorderitem.DisplayColumns += string.Format(",{0},{1},{2},{3},{4}", "quantity", "filterKey", "ItemMaxQuantity", "Code", "Total");
                else
                    scriptorderitem.DisplayColumns += string.Format(",{0},{1},{2},{3}", "quantity", "filterKey", "ItemMaxQuantity", "Code");

                string[] columns = scriptorderitem.DisplayColumns.Split(',');
                DataTable dataTable = BuildDisplayColumnsTable(columns, scriptorderitem, ContCont.HttpContext.Session);

                if (dataTable.Rows.Count > 0)
                    dataTable = dataTable.Select(string.Empty, "Code asc").CopyToDataTable();

                //ContCont.HttpContext.Session["DataTable"] = dataTable;

                var Model = new QueryModel();
                Model.Data = dataTable;

                //Column description: Name and Type
                var dyn = new Dictionary<string, System.Type>();
                foreach (System.Data.DataColumn column in Model.Data.Columns)
                {
                    var t = System.Type.GetType(column.DataType.FullName);
                    dyn.Add(column.ColumnName, t);
                }

                Model.Bind = dyn;

                List<ScriptItem> scriptItems = GetScriptItems(scriptorderitem);
                bool firsttime = true;
                string ilist = "";

                foreach (ScriptItem scriptItem in scriptItems)
                {
                    if (firsttime)
                    { ilist = scriptItem.ItemCode; }
                    else
                    { ilist = ilist + "~~" + scriptItem.ItemCode; }
                    firsttime = false;
                }

                ViewBag.itemCollection = ilist;
                if (!string.IsNullOrEmpty(scriptorderitem.SetKeys))
                {

                    ViewBag.SetKeys = "Y";
                    criticalelement = true;
                }
                else
                {

                    ViewBag.SetKeys = false;
                }

                if (criticalelement)
                {
                    ViewBag.OID = "C_" + scriptorderitem.ScriptItemSelectorID.ToString();
                    ViewBag.Critical = "Y";
                }
                else
                {
                    ViewBag.OID = "N_" + scriptorderitem.ScriptItemSelectorID.ToString();
                    ViewBag.Critical = "N";

                }
                ViewBag.Title = scriptorderitem.SelectorTitle;
                if (scriptItems.Count == 1)
                {
                    return RenderHelper.RenderViewToString(ContCont, "~/Views/OrderItem/_MultiItemGrid.cshtml", Model, ViewData);

                }
                else if (scriptItems.Count > 1)
                {

                    switch (scriptorderitem.LayoutType)
                    {
                        case "S":
                            return RenderHelper.RenderViewToString(ContCont, "~/Views/OrderItem/_SelectItem.cshtml", Model, ViewData);
                        case "M":
                            return RenderHelper.RenderViewToString(ContCont, "~/Views/OrderItem/_SelectMultiItem.cshtml", Model, ViewData);
                        case "F":
                            return RenderHelper.RenderViewToString(ContCont, "~/Views/OrderItem/_MultiItemGrid.cshtml", Model, ViewData);
                    }
                }
            }

            return "";
        }

        private DataTable BuildDisplayColumnsTable(string[] columns, ScriptItemSelector scriptorderitem, HttpSessionStateBase theSession)
        {
            string filterkeylist = "";
            DataTable dataTable = new DataTable();
            List<ScriptItem> ScriptItems = GetScriptItems(scriptorderitem);

            List<ItemOrdered> Items = SessionControl.SessionManager.GetItemsOrdered(theSession);

            foreach (ScriptItem sItem in ScriptItems)
            {
                //DisplayOrderItem DOI = new DisplayOrderItem();
                //DOI.OrderItem = sItem;

                DataRow dataRow = dataTable.NewRow();

                foreach (string col in columns)
                {
                    if (!dataTable.Columns.Contains(col))
                    {
                        if (col == "ItemPrice" || col == "ItemShippingPrice" || col == "ItemHandlingPrice")
                            dataTable.Columns.Add(col, typeof(Double));
                        else
                            dataTable.Columns.Add(col);
                    }

                    if (sItem.GetType().GetProperty(col) != null)
                    {
                        dataRow[col] = sItem.GetType().GetProperty(col).GetValue(sItem, null);
                    }
                    else 
                    {
                        foreach (var item in sItem.ScriptItemKeys)
                        {
                            if (col == item.ItemKey)
                                dataRow[col] = item.ItemValue;
                        }
                    }
                }

                dataRow["Code"] = sItem.ItemCode;
                dataRow["ItemMaxQuantity"] = sItem.ItemMaxQuantity;

                if (!string.IsNullOrEmpty(scriptorderitem.FilterKeys))
                {
                    List<string> oicKeys = scriptorderitem.FilterKeys.Split(',').ToList();
                    filterkeylist = "";

                    foreach (var item in oicKeys)
                    {
                        foreach (var itemKey in sItem.ScriptItemKeys)
                        {
                            if (item == itemKey.ItemKey)
                            {
                                if (filterkeylist != "") { filterkeylist += ","; }
                                filterkeylist += string.Format("{0}:{1}", itemKey.ItemKey, itemKey.ItemValue);
                            }
                        }
                    }

                    dataRow["filterKey"] = filterkeylist;
                }
                else
                {
                    dataRow["filterKey"] = "";
                }

                try
                {
                    string checkcode = sItem.ItemCode;
                    dataRow["quantity"] = "0";

                    foreach (ItemOrdered item in Items)
                    {
                        if (sItem.ItemCode == item.ItemCode)
                        {
                            dataRow["quantity"] = item.ItemQuantity.ToString();
                        }
                    }
                }
                catch
                { }

                if (scriptorderitem.LayoutType == "F")
                {
                    if (sItem.ItemMaxQuantity > 1)
                        dataRow["Total"] = string.Format("<input type=\"number\" min=\"0\" max=\"{0}\" value=\"{1}\" name=\"SPitemquant_{2}_{3}\" style=\"width: 35px\" />", sItem.ItemMaxQuantity.ToString(), dataRow["quantity"].ToString(), ViewBag.OID, dataRow["Code"].ToString());
                    else if (sItem.ItemMaxQuantity == 0)
                        dataRow["Total"] = string.Format("<input type=\"number\" min=\"0\" value=\"{0}\" name=\"SPitemquant_{1}_{2}\" style=\"width: 75px\" />", dataRow["quantity"].ToString(), ViewBag.OID, dataRow["Code"].ToString());
                    else
                        dataRow["Total"] = string.Format("<input type=\"checkbox\" name=\"SPitemcheck_{0}_{1}\" {2} />", ViewBag.OID, dataRow["Code"].ToString(), System.Convert.ToBoolean(Convert.ToInt32(dataRow["quantity"])) ? "checked" : string.Empty);

                }

                if (filterkeylist != "")
                {
                    if (KeyPassedFilter(filterkeylist, SessionControl.SessionManager.ReturnItemKeys(theSession)))
                    {
                        dataTable.Rows.Add(dataRow);
                    }
                }
                else
                {
                    dataTable.Rows.Add(dataRow);
                }

            }

            return dataTable;
        }

        public List<ScriptItem> GetScriptItems(ScriptItemSelector scriptorderitem)
        {
            string[] Items = System.Text.RegularExpressions.Regex.Split(scriptorderitem.ItemList, ",");

            List<ScriptItem> scriptItems = new List<ScriptItem>();
            API.ItemController itemC = new API.ItemController();

            foreach (string item in Items)
            {
                string[] itemparts = item.Split(new char[] { '\\' });
                switch (itemparts.Length)
                {
                    case 1:  //Category Selected
                        scriptItems.AddRange(itemC.GetActiveScriptItems(itemparts[0]));
                        break;
                    case 2:
                        scriptItems.AddRange(itemC.GetActiveScriptItems(itemparts[0], itemparts[1]));
                        break;
                    case 3:    //Item Selected
                        string[] itempieces = System.Text.RegularExpressions.Regex.Split(itemparts[2], "::");

                        if (itemC.GetActiveScriptItem(itemparts[0], itemparts[1], itempieces[0]) != null)
                            scriptItems.Add(itemC.GetActiveScriptItem(itemparts[0], itemparts[1], itempieces[0]));

                        break;
                    default:
                        break;
                }
            }

            return scriptItems;
        }

        public ActionResult  Grid_Read([DataSourceRequest]DataSourceRequest request, string id)
        {
            API.Elements.OrderItemController OC = new API.Elements.OrderItemController();
            string[] ids = id.Split('_');
            var actionResult = OC.GetScriptOrderItem(Convert.ToInt32(ids[1]));

            ViewBag.OID = id;

            ScriptItemSelector scriptorderitem = (actionResult as OkNegotiatedContentResult<ScriptItemSelector>).Content;

            if (scriptorderitem.LayoutType == "F")
                scriptorderitem.DisplayColumns += string.Format(",{0},{1},{2},{3},{4}", "quantity", "filterKey", "ItemMaxQuantity", "Code", "Total");
            else
                scriptorderitem.DisplayColumns += string.Format(",{0},{1},{2},{3}", "quantity", "filterKey", "ItemMaxQuantity", "Code");

            string[] columns = scriptorderitem.DisplayColumns.Split(',');
            DataTable dataTable = BuildDisplayColumnsTable(columns, scriptorderitem, HttpContext.Session);

            if (dataTable.Rows.Count > 0)
                dataTable = dataTable.Select(string.Empty, "Code asc").CopyToDataTable();

            return Json(dataTable.ToDataSourceResult(request));
        }

        //public List<DisplayOrderItem> GetAllOrderItems(ScriptItemSelector OIC,HttpSessionStateBase theSession)
        //{
        //    List<DisplayOrderItem> DisplayItems = new List<DisplayOrderItem>(); 
        //    string ItemList = OIC.ItemList;

        //    string setkeylist = "";
        //    string filterkeylist = "";

        //    string[] IndItems = System.Text.RegularExpressions.Regex.Split(ItemList, ",");

        //    List<ScriptItem> ScriptItems = new List<ScriptItem>();
        //    API.ItemController itemC = new API.ItemController();

        //    foreach (string indItem in IndItems)
        //    {
        //        string[] itemparts = indItem.Split(new char[] { '\\' });
        //        switch (itemparts.Length)
        //        {
        //            case 1:  //Category Selected

        //                ScriptItems.AddRange(itemC.GetActiveScriptItems(itemparts[0]));

        //                break;
        //            case 2:   
        //                ScriptItems.AddRange(itemC.GetActiveScriptItems(itemparts[0],itemparts[1]));
                    

        //                break;
        //            case 3:    //Item Selected

        //                string[] itempieces = System.Text.RegularExpressions.Regex.Split(itemparts[2], "::");

        //                if (itemC.GetActiveScriptItem(itemparts[0], itemparts[1], itempieces[0]) != null)
        //                    ScriptItems.Add(itemC.GetActiveScriptItem(itemparts[0],itemparts[1],itempieces[0]));

        //                break;


        //            default:

        //                break;
        //        }


        //    }
        //    List<ItemOrdered> Items = SessionControl.SessionManager.GetItemsOrdered(theSession);

        //    foreach (ScriptItem sItem in ScriptItems)
        //     {
        //        DisplayOrderItem DOI = new DisplayOrderItem();
        //        DOI.OrderItem = sItem;

        //        if (!string.IsNullOrEmpty(OIC.FilterKeys)) //&& sItem.ItemSetKeys != null
        //        {
        //            List<string> oicKeys = OIC.FilterKeys.Split(',').ToList();
        //            //filterkeylist = itemC.FilterKeyList(oicKeys);
        //            //string KeysAssoc =  sItem.ItemSetKeys;
        //            //if (KeysAssoc != "")
        //            //{
        //            //    string[] KeyList = Regex.Split(KeysAssoc, ",");

        //            //    filterkeylist = "";

        //            //    foreach (string keypair in KeyList)
        //            //    {
        //            //        string thekey = Regex.Split(keypair, ":")[0];
        //            //        string thevalue = Regex.Split(keypair, ":")[1];


        //            //        foreach (string filterkey in Regex.Split(OIC.KeyFilters, ","))
        //            //        {
        //            //            if (filterkey == thekey)
        //            //            {
        //            //                if (filterkeylist != "") { filterkeylist += ","; }
        //            //                filterkeylist += keypair;
        //            //            }

        //            //        }

        //            //    }
        //            //}
        //            DOI.filterKey = filterkeylist;


        //        }
        //        else
        //        {
        //            DOI.filterKey = "";
        //        }

        //        try
        //        {

        //            string checkcode = sItem.ItemCode;
        //            DOI.quantity = 0;

        //            foreach (ItemOrdered item in Items)
        //            {
        //                if (DOI.OrderItem.ItemCode == item.ItemCode)
        //                {
        //                    DOI.quantity = item.ItemQuantity;
        //                }
        //            }
                    
        //        }
        //        catch
        //        { }

        //        if (filterkeylist != "")
        //        {
        //            if (KeyPassedFilter(filterkeylist, SessionControl.SessionManager.ReturnItemKeys(theSession)))
        //            {
        //                DisplayItems.Add(DOI);

        //            }
        //        }
        //        else
        //        {
        //            DisplayItems.Add(DOI);
        //        }

        //    }

        //    return DisplayItems;
        //}


        public static bool KeyPassedFilter(string filterkeylist, Dictionary<string,string> SetKeyList)
        {
            if (SetKeyList == null)
            {
                return true;
            }


            if (filterkeylist == "" || filterkeylist == null) { return true; }
            if (SetKeyList == null) { return true; }

            foreach (string keypair in Regex.Split(filterkeylist, ","))
            {
                foreach (string keyplus in SetKeyList.Keys)
                {
                    string key = keyplus.Remove(0, 2);
                    string thekey = Regex.Split(keypair, ":")[0];
                    string thekeyval = Regex.Split(keypair, ":")[1];

                    if (key == thekey)
                    {
                            bool foundkey = false;
                            List<string> thevals = Regex.Split(SetKeyList[keyplus],"~").ToList();
                            foreach (string val in thevals)
	                        {
                                if (thekeyval == val)
                                {
                                    foundkey = true;
                                    break;
                                }
                            }
                            if (foundkey == false) { return false; }
                    }

                }
            }          

            return true;
        }                   

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    public class QueryModel
    {
        public DataTable Data { get; set; }
        public Dictionary<string, System.Type> Bind { get; set; }
    }

}
