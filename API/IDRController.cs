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
namespace ScreenViewer.API
{
    public class IDRController : ApiController
    {
        private ScreenPlayCRMEntities db = new ScreenPlayCRMEntities();

        [ResponseType(typeof(IDRServiceQueue))]
        [HttpGet]
        public IHttpActionResult GetIDRServiceQueue(int id)
        {
            IDRServiceQueue queue = db.IDRServiceQueues.Where(x=> x.ContactId == id).FirstOrDefault();
            if (queue == null)
            {
                return NotFound();
            }

            return Ok(queue);
        }

        // POST api/Contact
        [ResponseType(typeof(IDRServiceQueue))]
        public IHttpActionResult PostIDRServiceQueue(IDRServiceQueue queue)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.IDRServiceQueues.Add(queue);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = queue.IDRServiceQueueId }, queue);
        }

        public IHttpActionResult PutIDRServiceQueue(IDRServiceQueue queue)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.IDRServiceQueues.Attach(queue);
            db.Entry(queue).State = EntityState.Modified;
            db.SaveChanges();
            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}
