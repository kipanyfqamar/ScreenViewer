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

namespace ScreenViewer.API
{
    public class WorkflowLayoutsController : ApiController
    {
        private ScreenPlayEntities db = new ScreenPlayEntities();

        // GET: api/WorkflowLayouts
        public IQueryable<WorkflowLayout> GetWorkflowLayouts()
        {
            return db.WorkflowLayouts;
        }

        // GET: api/WorkflowLayouts/5
        [ResponseType(typeof(WorkflowLayout))]
        public IHttpActionResult GetWorkflowLayout(int id)
        {
            WorkflowLayout workflowLayout = db.WorkflowLayouts.Find(id);
            if (workflowLayout == null)
            {
                return NotFound();
            }

            return Ok(workflowLayout);
        }

        // GET: api/WorkflowLayouts/5
        [ResponseType(typeof(String))]
        public IHttpActionResult GetWorkflowLayoutString(string WorkflowLayoutName,string ClientID)
        {

            var layout = db.WorkflowLayouts.Where(u => u.HTMLDesc == WorkflowLayoutName && u.ClientID == ClientID).FirstOrDefault();
            if (layout == null)
            {
                return NotFound();
            }

            return Ok(layout.HTMLContent);
        }

        // PUT: api/WorkflowLayouts/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutWorkflowLayout(int id, WorkflowLayout workflowLayout)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != workflowLayout.WorkflowHTMLID)
            {
                return BadRequest();
            }

            db.Entry(workflowLayout).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!WorkflowLayoutExists(id))
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

        // POST: api/WorkflowLayouts
        [ResponseType(typeof(WorkflowLayout))]
        public IHttpActionResult PostWorkflowLayout(WorkflowLayout workflowLayout)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.WorkflowLayouts.Add(workflowLayout);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = workflowLayout.WorkflowHTMLID }, workflowLayout);
        }

        // DELETE: api/WorkflowLayouts/5
        [ResponseType(typeof(WorkflowLayout))]
        public IHttpActionResult DeleteWorkflowLayout(int id)
        {
            WorkflowLayout workflowLayout = db.WorkflowLayouts.Find(id);
            if (workflowLayout == null)
            {
                return NotFound();
            }

            db.WorkflowLayouts.Remove(workflowLayout);
            db.SaveChanges();

            return Ok(workflowLayout);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool WorkflowLayoutExists(int id)
        {
            return db.WorkflowLayouts.Count(e => e.WorkflowHTMLID == id) > 0;
        }
    }
}