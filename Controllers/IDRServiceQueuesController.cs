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
    public class IDRServiceQueuesController : Controller
    {
        private ScreenPlayCRMEntities db = new ScreenPlayCRMEntities();

        // GET: IDRServiceQueues
        public ActionResult Index()
        {
            var iDRServiceQueues = db.IDRServiceQueues.Include(i => i.ContactRecord);
            return View(iDRServiceQueues.ToList());
        }

        // GET: IDRServiceQueues/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            IDRServiceQueue iDRServiceQueue = db.IDRServiceQueues.Find(id);
            if (iDRServiceQueue == null)
            {
                return HttpNotFound();
            }
            return View(iDRServiceQueue);
        }

        public ActionResult ServiceQueue()
        {
            var iDRServiceQueues = db.IDRServiceQueues.Include(i => i.ContactRecord);
            return PartialView("ServiceQueue", iDRServiceQueues.ToList());
        }


        // GET: IDRServiceQueues/Create
        public ActionResult Create()
        {
            ViewBag.ContactId = new SelectList(db.ContactRecords, "ContactId", "ClientCallId");
            return View();
        }

        // POST: IDRServiceQueues/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IDRServiceQueueId,ContactId,ClientCallId,ServiceQueue,Status,CreatedDate,ModifiedDate,ClientId")] IDRServiceQueue iDRServiceQueue)
        {
            if (ModelState.IsValid)
            {
                db.IDRServiceQueues.Add(iDRServiceQueue);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ContactId = new SelectList(db.ContactRecords, "ContactId", "ClientCallId", iDRServiceQueue.ContactId);
            return View(iDRServiceQueue);
        }

        // GET: IDRServiceQueues/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            IDRServiceQueue iDRServiceQueue = db.IDRServiceQueues.Find(id);
            if (iDRServiceQueue == null)
            {
                return HttpNotFound();
            }
            ViewBag.ContactId = new SelectList(db.ContactRecords, "ContactId", "ClientCallId", iDRServiceQueue.ContactId);
            return View(iDRServiceQueue);
        }

        // POST: IDRServiceQueues/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IDRServiceQueueId,ContactId,ClientCallId,ServiceQueue,Status,CreatedDate,ModifiedDate,ClientId")] IDRServiceQueue iDRServiceQueue)
        {
            if (ModelState.IsValid)
            {
                db.Entry(iDRServiceQueue).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ContactId = new SelectList(db.ContactRecords, "ContactId", "ClientCallId", iDRServiceQueue.ContactId);
            return View(iDRServiceQueue);
        }

        // GET: IDRServiceQueues/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            IDRServiceQueue iDRServiceQueue = db.IDRServiceQueues.Find(id);
            if (iDRServiceQueue == null)
            {
                return HttpNotFound();
            }
            return View(iDRServiceQueue);
        }

        // POST: IDRServiceQueues/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            IDRServiceQueue iDRServiceQueue = db.IDRServiceQueues.Find(id);
            db.IDRServiceQueues.Remove(iDRServiceQueue);
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
