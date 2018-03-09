using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ScreenViewer.Models;
using System.Configuration;
using ScreenViewer.API.ExternalData;
using ScreenViewer.SessionControl;

namespace ScreenViewer.Controllers
{
    public class SiteInfoController : Controller
    {
        //
        // GET: /LeadInfo/
        public ActionResult Index()
        {
            try
            {
                //DataObjectLoader DOL = new API.ExternalData.DataObjectLoader();
                DataObjects DisplayObject = (DataObjects)SessionManager.GetDataObject(HttpContext.Session, "GetCustomer_Response");

                var myType = GenerateClass.CompileResultType(DisplayObject.Details);
                dynamic myObject = Activator.CreateInstance(myType);

                DisplayObject.ReverseObjectMatch(string.Empty, myObject);

                DataObjects DisplayObjectS = (DataObjects)SessionManager.GetDataObject(HttpContext.Session, "GetLocation_Response");

                var myType2 = GenerateClass.CompileResultType(DisplayObject.Details);
                dynamic myObject2 = Activator.CreateInstance(myType);

                DisplayObjectS.ReverseObjectMatch(string.Empty, myObject2);

                ViewBag.CustID = myObject.CUST_NUMBER;
                ViewBag.CustName = myObject.NAME;

   
                ViewBag.SiteNumber = myObject2.SiteNo;
                ViewBag.SiteName = myObject2.Name;
                ViewBag.Address1 = myObject2.SiteAddressOne;
                ViewBag.Address2 = myObject2.SiteAddressTwo;
                ViewBag.City = myObject2.SiteCity;
                ViewBag.State = myObject2.SiteState;
                ViewBag.ZipCode = myObject2.SiteZipCode;

            }
            catch { }

            return PartialView("_SiteInfo");
        }

        public bool IsPropertyExist(dynamic settings, string name)
        {
            return settings.GetType().GetProperty(name) != null;
        }
    }
}