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

namespace ScreenViewer.API.Elements
{
    public class ActionController : ApiController
    {
        private ScreenPlayEntities db = new ScreenPlayEntities();

        // GET api/Action
        public IQueryable<ScriptAction> GetScriptActions()
        {
            return db.ScriptActions;
        }

        // GET api/Action/5
        [ResponseType(typeof(ScriptAction))]
        public IHttpActionResult GetScriptAction(decimal id)
        {
            ScriptAction scriptaction = db.ScriptActions.Find(id);
            if (scriptaction == null)
            {
                return NotFound();
            }

            return Ok(scriptaction);
        }

        // PUT api/Action/5
        public IHttpActionResult PutScriptAction(decimal id, ScriptAction scriptaction)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != scriptaction.ScriptActionID)
            {
                return BadRequest();
            }

            db.Entry(scriptaction).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ScriptActionExists(id))
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

        // POST api/Action
        [ResponseType(typeof(ScriptAction))]
        public IHttpActionResult PostScriptAction(ScriptAction scriptaction)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.ScriptActions.Add(scriptaction);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = scriptaction.ScriptActionID }, scriptaction);
        }

        // DELETE api/Action/5
        [ResponseType(typeof(ScriptAction))]
        public IHttpActionResult DeleteScriptAction(decimal id)
        {
            ScriptAction scriptaction = db.ScriptActions.Find(id);
            if (scriptaction == null)
            {
                return NotFound();
            }

            db.ScriptActions.Remove(scriptaction);
            db.SaveChanges();

            return Ok(scriptaction);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ScriptActionExists(decimal id)
        {
            return db.ScriptActions.Count(e => e.ScriptActionID == id) > 0;
        }
    }
}