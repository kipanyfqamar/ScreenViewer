using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using ScreenViewer.Data;

namespace ScreenViewer.API.ExternalData
{
    public class LeadRecordController : ApiController
    {
        private ScreenPlayCRMEntities db = new ScreenPlayCRMEntities();

        // GET api/LeadRecord
        public IQueryable<LeadRecord> GetLeadRecords()
        {
            return db.LeadRecords;
        }

        // GET api/LeadRecord/5
        [ResponseType(typeof(LeadRecord))]
        public IHttpActionResult GetLeadRecord(string id)
        {
            LeadRecord leadrecord = db.LeadRecords.Find(id);
            if (leadrecord == null)
            {
                return NotFound();
            }

            return Ok(leadrecord);
        }

        [ResponseType(typeof(LeadRecord))]
        public IHttpActionResult GetLeadRecordByColumn(string columnName, string columnValue)
        {
            LeadRecord leadrecord = null;
            if (columnName.ToUpper().Equals("LEADRECORDID"))
            {
                leadrecord = db.LeadRecords.Find(columnValue);
            }
            else
            {
                if (columnName.Equals("Phone"))
                    leadrecord = db.LeadRecords.Where(x => x.Phone == columnValue).FirstOrDefault();

                if (columnName.Equals("CustomId"))
                    leadrecord = db.LeadRecords.Where(x => x.CustomId == columnValue).FirstOrDefault();

                //var query = from res in db.LeadRecords.AsEnumerable()
                //            where res.Field<string>(columnName).Contains(columnValue)
                //            select res;
            }

            if (leadrecord == null)
            {
                return NotFound();
            }

            return Ok(leadrecord);
        }

        // PUT api/LeadRecord/5
        public IHttpActionResult PutLeadRecord(string id, LeadRecord leadrecord)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != leadrecord.LeadRecordId)
            {
                return BadRequest();
            }

            db.Entry(leadrecord).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LeadRecordExists(id))
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

        // POST api/LeadRecord
        [ResponseType(typeof(LeadRecord))]
        public IHttpActionResult PostLeadRecord(LeadRecord leadrecord)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.LeadRecords.Add(leadrecord);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException)
            {
                if (LeadRecordExists(leadrecord.LeadRecordId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = leadrecord.LeadRecordId }, leadrecord);
        }

        // DELETE api/LeadRecord/5
        [ResponseType(typeof(LeadRecord))]
        public IHttpActionResult DeleteLeadRecord(int id)
        {
            LeadRecord leadrecord = db.LeadRecords.Find(id);
            if (leadrecord == null)
            {
                return NotFound();
            }

            db.LeadRecords.Remove(leadrecord);
            db.SaveChanges();

            return Ok(leadrecord);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool LeadRecordExists(string id)
        {
            return db.LeadRecords.Count(e => e.LeadRecordId == id) > 0;
        }
    }
}