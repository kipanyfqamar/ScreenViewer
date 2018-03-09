using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Http.Results;
using ScreenViewer.API;
using ScreenViewer.API.ExternalData;
using ScreenViewer.Data;
using ScreenViewer.Models;
using ScreenViewer.Models.ExternalData;
using ScreenViewer.SessionControl;

namespace ScreenViewer.Controllers
{
    public class DataViewController : Controller
    {
        //
        // GET: /DataView/
        public ActionResult Index()
        {
            return View();
        }

        //HttpContext.Session
        // GET: /DataView/Details/5
        public ActionResult Display(decimal id)
        {

            ViewBag.DataViewID = id;

            ScreenViewer.API.Elements.DataViewController DVC = new API.Elements.DataViewController();
            var actionResult = DVC.GetScriptDataView(id);



            var response = actionResult as OkNegotiatedContentResult<Data.ScriptDataView>;
            //Assert.IsNotNull(response);
            Data.ScriptDataView theDataView = response.Content;
            DataObjectManager DOM = new DataObjectManager();
            ScreenViewer.API.DataObjectController DOC = new API.DataObjectController();
            switch (theDataView.DataViewType)
            {
                case "R":
                    DataObjectLoader DOL = new API.ExternalData.DataObjectLoader();

                    string objectname = System.Text.RegularExpressions.Regex.Split(theDataView.DataViewSource, "::")[1];

                    DataObjects DisplayObject = (DataObjects)SessionManager.GetDataObject(HttpContext.Session, objectname);

                    //Models.DataObjects DisplayObject = (Models.DataObjects)SessionControl.SessionManager.GetDataObject(HttpContext.Session,theDataView.DataViewObjectSource);
                    if (DisplayObject != null)
                    {
                        return PartialView("_DataView_Record", DisplayObject.Details);
                    }

                    break;
                case "C":

                    string objectcolletion = System.Text.RegularExpressions.Regex.Split(theDataView.DataViewSource, "::")[1];
                    string dataobname = System.Text.RegularExpressions.Regex.Split(objectcolletion, "\\.")[0];
                    string tabledata = SPutilities.GenerateGridData2(objectcolletion, HttpContext.Session);
                    ViewBag.Table = tabledata;
                    return PartialView("_DataView_Grid", ViewData);

                case "S":

                    break;
                default:
                    break;
            }


            return View();
        }

        public string Render(decimal id, ControllerContext ContCont)
        {
            ViewBag.DataViewID = id;

            ScreenViewer.API.Elements.DataViewController DVC = new API.Elements.DataViewController();
            var actionResult = DVC.GetScriptDataView(id);

            var response = actionResult as OkNegotiatedContentResult<Data.ScriptDataView>;
            Data.ScriptDataView theDataView = response.Content;
            DataObjectManager DOM = new DataObjectManager();
            ScreenViewer.API.DataObjectController DOC = new API.DataObjectController();

            ViewBag.OID = response.Content.ScriptDataViewID.ToString();

            switch (theDataView.DataViewType)
            {
                case "R":
                    DataObjectLoader DOL = new API.ExternalData.DataObjectLoader();

                    string objectname = System.Text.RegularExpressions.Regex.Split(theDataView.DataViewSource, "::")[1];
                    DataObjects DisplayObject = (DataObjects)SessionManager.GetDataObject(ContCont.HttpContext.Session, objectname);

                    //Models.DataObjects DisplayObject = (Models.DataObjects)SessionControl.SessionManager.GetDataObject(HttpContext.Session,theDataView.DataViewObjectSource);
                    if (DisplayObject != null)
                    {
                        return RenderHelper.RenderViewToString(ContCont, "~/Views/DataView/_DataView_Record.cshtml", DisplayObject.Details, ViewData);
                        //return PartialView("_DataView_Record", DisplayObject.Details);
                    }
                    else
                    {
                        return string.Empty;
                    }
                case "C":

                    string objectcolletion = System.Text.RegularExpressions.Regex.Split(theDataView.DataViewSource, "::")[1];
                    string dataobname = System.Text.RegularExpressions.Regex.Split(objectcolletion, "\\.")[0];

                    if (SessionManager.GetDataObject(ContCont.HttpContext.Session, dataobname) != null)
                    {
                        string tabledata = SPutilities.GenerateGridData3(objectcolletion, ContCont.HttpContext.Session);
                        ViewBag.Table = tabledata;
                        return RenderHelper.RenderViewToString(ContCont, "~/Views/DataView/_DataView_Grid.cshtml", null, ViewData);
                    }
                    else
                    {
                        ViewBag.Table = string.Empty;
                        return RenderHelper.RenderViewToString(ContCont, "~/Views/DataView/_DataView_Grid.cshtml", null, ViewData);
                    }

                case "S":
                    return string.Empty;
                default:
                    return string.Empty;
            }
        }

        //
        // GET: /DataView/Create
        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /DataView/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        //GET: /DataView/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /DataView/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /DataView/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /DataView/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
