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
    public class WorkflowController : ApiController
    {
        private ScreenPlayEntities db = new ScreenPlayEntities();

        // GET api/Workflow
        public IQueryable<ScriptWorkflow> GetScriptWorkflows()
        {
            return db.ScriptWorkflows;
        }

        // GET api/Workflow/5
        [ResponseType(typeof(ScriptWorkflow))]
        public IHttpActionResult GetScriptWorkflow(int id)
        {
            ScriptWorkflow scriptworkflow = db.ScriptWorkflows.Find(id);
            if (scriptworkflow == null)
            {
                return NotFound();
            }

            return Ok(scriptworkflow);
        }

        public int GetScriptWorkflowId(string name)
        {
            return db.ScriptWorkflows.Where(b => b.WorkflowName.Equals(name)).FirstOrDefault().ScriptWorkflowID;
        }

        // PUT api/Workflow/5
        public IHttpActionResult PutScriptWorkflow(int id, ScriptWorkflow scriptworkflow)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != scriptworkflow.ScriptWorkflowID)
            {
                return BadRequest();
            }

            db.Entry(scriptworkflow).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ScriptWorkflowExists(id))
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

        // POST api/Workflow
        [ResponseType(typeof(ScriptWorkflow))]
        public IHttpActionResult PostScriptWorkflow(ScriptWorkflow scriptworkflow)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.ScriptWorkflows.Add(scriptworkflow);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = scriptworkflow.ScriptWorkflowID }, scriptworkflow);
        }

        // DELETE api/Workflow/5
        [ResponseType(typeof(ScriptWorkflow))]
        public IHttpActionResult DeleteScriptWorkflow(int id)
        {
            ScriptWorkflow scriptworkflow = db.ScriptWorkflows.Find(id);
            if (scriptworkflow == null)
            {
                return NotFound();
            }

            db.ScriptWorkflows.Remove(scriptworkflow);
            db.SaveChanges();

            return Ok(scriptworkflow);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ScriptWorkflowExists(int id)
        {
            return db.ScriptWorkflows.Count(e => e.ScriptWorkflowID == id) > 0;
        }
    }
}