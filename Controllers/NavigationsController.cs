using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ScreenViewer.SessionControl;
using ScreenViewer.Data;

namespace ScreenViewer.Controllers
{
    public class NavigationsController : Controller
    {
        private ScreenPlayCRMEntities db = new ScreenPlayCRMEntities();

        // GET: Navigations
        //public ActionResult Index()
        //{
        //    var contactNavigations = db.ContactNavigations.Include(c => c.ContactRecord);
        //    return View(contactNavigations.ToList());
        //}

        // GET: Navigations/Details/5
        public ActionResult Index()
        {
            int contactId = Convert.ToInt32(SessionManager.GetScriptParameterByKey("PreviousCallID", Session));
            string clientId = SessionControl.SessionManager.GetClientId(HttpContext.Session);

            //ContactNavigation contactNavigation = db.ContactNavigations.Find(id);
            var contactNavigations = db.ContactNavigations.Where(x=> x.ContactId == contactId && x.ClientId == clientId);
            if (contactNavigations == null)
            {
                return HttpNotFound();
            }
            return View(contactNavigations.ToList());
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
}
