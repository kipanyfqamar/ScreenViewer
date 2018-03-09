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
    public class ScriptLayoutsController : ApiController
    {
        private ScreenPlayEntities db = new ScreenPlayEntities();

        // GET: api/ScriptLayouts
        public IQueryable<ScriptLayout> GetScriptLayouts()
        {
            return db.ScriptLayouts;
        }

        // GET: api/ScriptLayouts/5
        [ResponseType(typeof(ScriptLayout))]
        public IHttpActionResult GetScriptLayout(int id)
        {
            ScriptLayout scriptLayout = db.ScriptLayouts.Find(id);
            if (scriptLayout == null)
            {
                return NotFound();
            }

            return Ok(scriptLayout);
        }

        // PUT: api/ScriptLayouts/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutScriptLayout(int id, ScriptLayout scriptLayout)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != scriptLayout.ScriptLayoutID)
            {
                return BadRequest();
            }

            db.Entry(scriptLayout).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ScriptLayoutExists(id))
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

        // POST: api/ScriptLayouts
        [ResponseType(typeof(ScriptLayout))]
        public IHttpActionResult PostScriptLayout(ScriptLayout scriptLayout)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.ScriptLayouts.Add(scriptLayout);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = scriptLayout.ScriptLayoutID }, scriptLayout);
        }

        // DELETE: api/ScriptLayouts/5
        [ResponseType(typeof(ScriptLayout))]
        public IHttpActionResult DeleteScriptLayout(int id)
        {
            ScriptLayout scriptLayout = db.ScriptLayouts.Find(id);
            if (scriptLayout == null)
            {
                return NotFound();
            }

            db.ScriptLayouts.Remove(scriptLayout);
            db.SaveChanges();

            return Ok(scriptLayout);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ScriptLayoutExists(int id)
        {
            return db.ScriptLayouts.Count(e => e.ScriptLayoutID == id) > 0;
        }
    }
}