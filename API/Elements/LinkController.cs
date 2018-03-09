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
    public class LinkController : ApiController
    {
        private ScreenPlayEntities db = new ScreenPlayEntities();

        // GET api/Link
        public IQueryable<ScriptLink> GetScriptLinks()
        {
            return db.ScriptLinks;
        }

        // GET api/Link/5
        [ResponseType(typeof(ScriptLink))]
        public IHttpActionResult GetScriptLink(decimal id)
        {
            ScriptLink scriptlink = db.ScriptLinks.Find(id);
            if (scriptlink == null)
            {
                return NotFound();
            }

            return Ok(scriptlink);
        }

        // PUT api/Link/5
        public IHttpActionResult PutScriptLink(decimal id, ScriptLink scriptlink)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != scriptlink.ScriptLinkID)
            {
                return BadRequest();
            }

            db.Entry(scriptlink).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ScriptLinkExists(id))
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

        // POST api/Link
        [ResponseType(typeof(ScriptLink))]
        public IHttpActionResult PostScriptLink(ScriptLink scriptlink)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.ScriptLinks.Add(scriptlink);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = scriptlink.ScriptLinkID }, scriptlink);
        }

        // DELETE api/Link/5
        [ResponseType(typeof(ScriptLink))]
        public IHttpActionResult DeleteScriptLink(decimal id)
        {
            ScriptLink scriptlink = db.ScriptLinks.Find(id);
            if (scriptlink == null)
            {
                return NotFound();
            }

            db.ScriptLinks.Remove(scriptlink);
            db.SaveChanges();

            return Ok(scriptlink);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ScriptLinkExists(decimal id)
        {
            return db.ScriptLinks.Count(e => e.ScriptLinkID == id) > 0;
        }
    }
}