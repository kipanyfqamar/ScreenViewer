using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.Mvc;
using System.Reflection;
using ScreenViewer.API.CRM;
using ScreenViewer.Data;
using System.ComponentModel;
using ScreenViewer.Controllers;


namespace ScreenViewer
{
    public class ApiExplorerController : Controller
    {
        // GET: /ApiExplorer/
        public ActionResult Index(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                ContactController contactController = new ContactController();
                var contactResult = contactController.GetSPContact(Convert.ToInt32(id));

                if (contactResult != null && contactResult != contactResult as System.Web.Http.Results.NotFoundResult)
                {
                    var objectResponse = contactResult as OkNegotiatedContentResult<ContactRecord>;
                    ContactRecord contact = (ContactRecord)objectResponse.Content;
                    ViewBag.Contact = contact;
                    ViewBag.Orders = contact.Orders.Count > 0 ? contact.Orders.First() : null;
                }
                else
                {
                    ViewBag.Message = string.Format("No Contact found!");
                }
            }
            return View("Explorer");
        }

        [System.Web.Mvc.HttpPost]
        //[MultipleButton(Name = "action", Argument = "Invoke")]
        public ActionResult Invoke(FormCollection collection)
        {
            int contactId = Convert.ToInt32(Request["txtContactId"]);
            ContactController contactController = new ContactController();
            var contactResult = contactController.GetSPContact(contactId);

            if (contactResult != null && contactResult != contactResult as System.Web.Http.Results.NotFoundResult)
            {
                var objectResponse = contactResult as OkNegotiatedContentResult<ContactRecord>;
                ContactRecord contact = (ContactRecord)objectResponse.Content;
                ViewBag.Contact = contact;
                ViewBag.Orders = contact.Orders.Count > 0 ? contact.Orders.First() : null;
            }
            else
            {
                ViewBag.Message = string.Format("No Contact found!");
            }

            return View("Explorer");
        }
	}
}