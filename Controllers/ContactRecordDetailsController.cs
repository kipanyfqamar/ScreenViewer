using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ScreenViewer.Data;

namespace ScreenViewer.Controllers
{
    public class ContactRecordDetailsController : Controller
    {
        private ScreenPlayCRMEntities db = new ScreenPlayCRMEntities();

        // GET: ContactRecordDetails
        public ActionResult Index()
        {
            var contactRecordDetails = db.ContactRecordDetails.Include(c => c.ContactRecord);
            return View(contactRecordDetails.ToList());
        }

        // GET: ContactRecordDetails
        public ActionResult CallResponses()
        {
            int pcid = Convert.ToInt32(SessionControl.SessionManager.GetScriptParameterByKey("PreviousCallID", HttpContext.Session));
            var contactRecordDetails = db.ContactRecordDetails.Include(c => c.ContactRecord).Where(f=> f.ContactId==pcid);
            return View("Index",contactRecordDetails.ToList());
        }



        // GET: ContactRecordDetails/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ContactRecordDetail contactRecordDetail = db.ContactRecordDetails.Find(id);
            if (contactRecordDetail == null)
            {
                return HttpNotFound();
            }
            return View(contactRecordDetail);
        }

        // GET: ContactRecordDetails/Create
        public ActionResult Create()
        {
            ViewBag.ContactId = new SelectList(db.ContactRecords, "ContactId", "ClientCallId");
            return View();
        }

        // POST: ContactRecordDetails/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ContactRecordDetailId,ContactId,QuestionId,QuestionText,QuestionResponseText,QuestionResponseValue,QuestionKeys")] ContactRecordDetail contactRecordDetail)
        {
            if (ModelState.IsValid)
            {
                db.ContactRecordDetails.Add(contactRecordDetail);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ContactId = new SelectList(db.ContactRecords, "ContactId", "ClientCallId", contactRecordDetail.ContactId);
            return View(contactRecordDetail);
        }

        // GET: ContactRecordDetails/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ContactRecordDetail contactRecordDetail = db.ContactRecordDetails.Find(id);
            if (contactRecordDetail == null)
            {
                return HttpNotFound();
            }
            ViewBag.ContactId = new SelectList(db.ContactRecords, "ContactId", "ClientCallId", contactRecordDetail.ContactId);
            return View(contactRecordDetail);
        }

        // POST: ContactRecordDetails/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ContactRecordDetailId,ContactId,QuestionId,QuestionText,QuestionResponseText,QuestionResponseValue,QuestionKeys")] ContactRecordDetail contactRecordDetail)
        {
            if (ModelState.IsValid)
            {
                db.Entry(contactRecordDetail).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ContactId = new SelectList(db.ContactRecords, "ContactId", "ClientCallId", contactRecordDetail.ContactId);
            return View(contactRecordDetail);
        }

        // GET: ContactRecordDetails/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ContactRecordDetail contactRecordDetail = db.ContactRecordDetails.Find(id);
            if (contactRecordDetail == null)
            {
                return HttpNotFound();
            }
            return View(contactRecordDetail);
        }

        // POST: ContactRecordDetails/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ContactRecordDetail contactRecordDetail = db.ContactRecordDetails.Find(id);
            db.ContactRecordDetails.Remove(contactRecordDetail);
            db.SaveChanges();
            return RedirectToAction("Index");
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
