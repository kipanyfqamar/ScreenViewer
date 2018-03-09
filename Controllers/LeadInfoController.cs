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
    public class LeadInfoController : Controller
    {
        //
        // GET: /LeadInfo/
        public ActionResult Index()
        {
            try
            {
                DataObjectLoader DOL = new API.ExternalData.DataObjectLoader();
                DataObjects DisplayObject = (DataObjects)SessionManager.GetDataObject(HttpContext.Session, "SXMLead");

                var myType = GenerateClass.CompileResultType(DisplayObject.Details);
                dynamic myObject = Activator.CreateInstance(myType);

                DisplayObject.ReverseObjectMatch(string.Empty, myObject);


                ViewBag.LeadName = string.Format("{0} {1}", myObject.FIRST_NAME, myObject.LAST_NAME);
                ViewBag.LeadAddress = string.Format("{0} {1}\n{2} {3}", myObject.ADDRESS1, myObject.ADDRESS2, myObject.CITY, myObject.STATE);
                ViewBag.ACCOUNT_NUMBER = myObject.ACCOUNT_NUMBER;
                ViewBag.Email = myObject.EMAIL;
                ViewBag.PLAN_DESCRIPTION = myObject.PLAN_DESCRIPTION;
                ViewBag.PLAN_START_DATE = !string.IsNullOrEmpty(myObject.PLAN_START_DATE) ? Convert.ToDateTime(myObject.PLAN_START_DATE).ToString("MM/dd/yyyy") : string.Empty;
                ViewBag.NEXT_RENEWAL_DATE = !string.IsNullOrEmpty(myObject.NEXT_RENEWAL_DATE) ? Convert.ToDateTime(myObject.NEXT_RENEWAL_DATE).ToString("MM/dd/yyyy") : string.Empty;
                ViewBag.ESN_DEACTIVATION_DATE = !string.IsNullOrEmpty(myObject.ESN_DEACTIVATION_DATE) ? Convert.ToDateTime(myObject.ESN_DEACTIVATION_DATE).ToString("MM/dd/yyyy") : string.Empty;
                ViewBag.AUTO_MAKE = myObject.AUTO_MAKE;
                ViewBag.AUTO_MODEL = myObject.AUTO_MODEL;
                ViewBag.Auto_Trim_Year = string.Format("{0} {1}", myObject.Auto_Trim, myObject.Auto_Year);
            }
            catch { }

            return PartialView("_LeadInfo");
        }

        public bool IsPropertyExist(dynamic settings, string name)
        {
            return settings.GetType().GetProperty(name) != null;
        }
    }
}