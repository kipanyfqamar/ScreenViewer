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
    public class ScriptDashesController : ApiController
    {
        private ScreenPlayEntities db = new ScreenPlayEntities();

        // GET: api/ScriptDashes
        public IQueryable<ScriptDash> GetScriptDashes()
        {
            return db.ScriptDashes;
        }

        // GET: api/ScriptDashes/5
        [ResponseType(typeof(ScriptDash))]
        public IHttpActionResult GetScriptDash(int id)
        {
            ScriptDash scriptDash = db.ScriptDashes.Find(id);
            if (scriptDash == null)
            {
                return NotFound();
            }

            return Ok(scriptDash);
        }

        public int GetDashboardId(string name)
        {
            try
            {
                return db.ScriptDashes.Where(b => b.ProjectName.Equals(name)).FirstOrDefault().ScriptDashID;
            }
            catch (Exception ex)
            {
                throw new System.Web.HttpException(404, "Not found");
            }
        }

        // PUT: api/ScriptDashes/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutScriptDash(int id, ScriptDash scriptDash)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != scriptDash.ScriptDashID)
            {
                return BadRequest();
            }

            db.Entry(scriptDash).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ScriptDashExists(id))
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

        // POST: api/ScriptDashes
        [ResponseType(typeof(ScriptDash))]
        public IHttpActionResult PostScriptDash(ScriptDash scriptDash)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.ScriptDashes.Add(scriptDash);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = scriptDash.ScriptDashID }, scriptDash);
        }

        // DELETE: api/ScriptDashes/5
        [ResponseType(typeof(ScriptDash))]
        public IHttpActionResult DeleteScriptDash(int id)
        {
            ScriptDash scriptDash = db.ScriptDashes.Find(id);
            if (scriptDash == null)
            {
                return NotFound();
            }

            db.ScriptDashes.Remove(scriptDash);
            db.SaveChanges();

            return Ok(scriptDash);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ScriptDashExists(int id)
        {
            return db.ScriptDashes.Count(e => e.ScriptDashID == id) > 0;
        }
    }
}