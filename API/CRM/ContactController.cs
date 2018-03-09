using System;
using System.Collections.Generic;
using System.Globalization;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using ScreenViewer.Data;

namespace ScreenViewer.API.CRM
{
    public class ContactController : ApiController
    {
        private ScreenPlayCRMEntities db = new ScreenPlayCRMEntities();

        // GET api/Contact
        public IQueryable<ContactRecord> GetContactRecords()
        {
            return db.ContactRecords;
        }

        // GET api/Contact/5

        [ResponseType(typeof(ContactRecord))]
        [HttpGet]
        [ActionName("GetSPContact")]
        public IHttpActionResult GetSPContact(int id)
        {
            ContactRecord contactrecord = db.ContactRecords.Find(id);
            if (contactrecord == null)
            {
                return NotFound();
            }

            return Ok(contactrecord);
        }

        [ResponseType(typeof(ContactRecord))]
        [HttpGet]
        public IHttpActionResult GetFirstContact(string id, string clientId)
        {
            ContactRecord contactrecord = db.ContactRecords.Where(b => b.ClientCallId.Equals(id) && b.ClientId.Equals(clientId)).FirstOrDefault();
            if (contactrecord == null)
            {
                return NotFound();
            }

            return Ok(contactrecord);
        }

        [HttpGet]
        public IQueryable<ContactRecord> GetContacts(string id,string clientId)
        {
            DateTime today = System.DateTime.Now;
            return db.ContactRecords.Where(b => b.ClientCallId.Equals(id) && b.ClientId.Equals(clientId));
        }

        [HttpGet]
        [ResponseType(typeof(IQueryable<ContactRecord>))]
        [ActionName("GetTodaysContacts")]
        public IHttpActionResult GetTodaysContacts(string id)
        {
            try
            {
                var today = System.DateTime.Now.Date;
                return Ok(db.ContactRecords.Where(b => b.CallStartDateTime.Value >= today && b.ClientId.Equals(id)));
            }
            catch
            {
                return BadRequest();

            }
        }


        [HttpGet]
        [ResponseType(typeof(IQueryable<ContactRecord>))]
        [ActionName("GetYesterdaysContacts")]
        public IHttpActionResult GetYesterdaysContacts(string id)
        {
            try
            {
                var today = System.DateTime.Now.Date;
                var yesterday = System.DateTime.Now.Date.AddDays(-1);
                return Ok(db.ContactRecords.Where(b => b.CallStartDateTime.Value >= yesterday && b.CallStartDateTime < today && b.ClientId.Equals(id)));
            }
            catch
            {
                return BadRequest();
            }
        }

        [HttpGet]
        [ResponseType(typeof(IQueryable<ContactRecord>))]
        public IHttpActionResult GetRangeContacts(string startdate, string enddate, string clientId)
        {
            try
            {
                DateTime tempdate;
                DateTime tempdate2;

                if (startdate.Length == 10)
                {
                    tempdate = DateTime.ParseExact(startdate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                }
                else if (startdate.Length == 16)
                {
                    tempdate = DateTime.ParseExact(startdate, "yyyy-MM-dd-HH-mm", CultureInfo.InvariantCulture);
                }
                else
                { return BadRequest(); }
                if (enddate.Length == 10)
                {
                    tempdate2 = DateTime.ParseExact(enddate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                }
                else if (enddate.Length == 16)
                {
                    tempdate2 = DateTime.ParseExact(enddate, "yyyy-MM-dd-HH-mm", CultureInfo.InvariantCulture);
                }
                else
                { return BadRequest(); }

                DateTime sdate;
                DateTime edate;

                if (tempdate < tempdate2)
                {
                    sdate = tempdate;
                    edate = tempdate2;

                }
                else
                {

                    sdate = tempdate2;
                    edate = tempdate;
                }

                return Ok(db.ContactRecords.Where(b => b.CallStartDateTime.Value >= sdate && b.CallStartDateTime < edate && b.ClientId.Equals(clientId)));
            }
            catch
            {
                return BadRequest();
            }
        }


        public IQueryable<ContactRecord> GetContact(string id)
        {
            return db.ContactRecords.Where(b => b.ClientCallId.Equals(id));
        }

        //[HttpGet]
        //public Array GetTodaysCallBack()
        //{
        //    DateTime today = DateTime.Today;                    
        //    DateTime tomorrow = DateTime.Today.AddDays(1);

        //    var callBackDates = db.ContactRecords
        //                .Where(x => x.CallbackDatetime >= today)
        //                .Where(x => x.CallbackDatetime < tomorrow)
        //                .Select(p => new { p.LeadRecordID, p.CallbackDatetime }).ToList();

        //    return callBackDates.ToArray();
        //}

        //[ResponseType(typeof(Array))]
        //[HttpGet]
        //public IHttpActionResult GetRangeCallBack(string startdate, string enddate)
        //{
        //    try
        //    {
        //        DateTime tempdate;
        //        DateTime tempdate2;

        //        if (startdate.Length == 10)
        //        {
        //            tempdate = DateTime.ParseExact(startdate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        //        }
        //        else if (startdate.Length == 16)
        //        {
        //            tempdate = DateTime.ParseExact(startdate, "yyyy-MM-dd-HH-mm", CultureInfo.InvariantCulture);
        //        }
        //        else
        //        { return BadRequest(); }
        //        if (enddate.Length == 10)
        //        {
        //            tempdate2 = DateTime.ParseExact(enddate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        //        }
        //        else if (enddate.Length == 16)
        //        {
        //            tempdate2 = DateTime.ParseExact(enddate, "yyyy-MM-dd-HH-mm", CultureInfo.InvariantCulture);
        //        }
        //        else
        //        { return BadRequest(); }

        //        DateTime sdate;
        //        DateTime edate;

        //        if (tempdate < tempdate2)
        //        {
        //            sdate = tempdate;
        //            edate = tempdate2;

        //        }
        //        else
        //        {

        //            sdate = tempdate2;
        //            edate = tempdate;
        //        }

        //        var callBackDates = db.ContactRecords
        //                .Where(x => x.CallbackDatetime >= sdate)
        //                .Where(x => x.CallbackDatetime < edate)
        //                .Select(p => new { p.LeadRecordID, p.CallbackDatetime }).ToList();


        //        return Ok(callBackDates);
        //    }
        //    catch
        //    {
        //        return BadRequest();
        //    }
        //}


        // PUT api/Contact/5
        public IHttpActionResult PutContactRecord(int id, ContactRecord contactrecord)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != contactrecord.ContactId)
            {
                return BadRequest();
            }

            db.Entry(contactrecord).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ContactRecordExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST api/Contact
        [ResponseType(typeof(ContactRecord))]
        public IHttpActionResult PostContactRecord(ContactRecord contactrecord)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.ContactRecords.Add(contactrecord);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = contactrecord.ContactId }, contactrecord);
        }

        // DELETE api/Contact/5
        [ResponseType(typeof(ContactRecord))]
        public IHttpActionResult DeleteContactRecord(int id)
        {
            ContactRecord contactrecord = db.ContactRecords.Find(id);
            if (contactrecord == null)
            {
                return NotFound();
            }

            db.ContactRecords.Remove(contactrecord);
            db.SaveChanges();

            return Ok(contactrecord);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ContactRecordExists(int id)
        {
            return db.ContactRecords.Count(e => e.ContactId == id) > 0;
        }
    }
}