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
    public class LinkGroupController : ApiController
    {
        private ScreenPlayEntities db = new ScreenPlayEntities();

        // GET api/LinkGroup
        public IQueryable<ScriptMenu> GetScriptLinkGroups()
        {
            return db.ScriptMenus;
        }

        // GET api/LinkGroup/5
        [ResponseType(typeof(ScriptMenu))]
        public IHttpActionResult GetScriptLinkGroup(int id)
        {
            ScriptMenu scriptlinkgroup = db.ScriptMenus.Find(id);
            if (scriptlinkgroup == null)
            {
                return NotFound();
            }

            return Ok(scriptlinkgroup);
        }

        // PUT api/LinkGroup/5
        public IHttpActionResult PutScriptLinkGroup(int id, ScriptMenu scriptlinkgroup)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != scriptlinkgroup.ScriptMenuID)
            {
                return BadRequest();
            }

            db.Entry(scriptlinkgroup).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ScriptLinkGroupExists(id))
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

        // POST api/LinkGroup
        [ResponseType(typeof(ScriptMenu))]
        public IHttpActionResult PostScriptLinkGroup(ScriptMenu scriptlinkgroup)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.ScriptMenus.Add(scriptlinkgroup);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = scriptlinkgroup.ScriptMenuID }, scriptlinkgroup);
        }

        // DELETE api/LinkGroup/5
        [ResponseType(typeof(ScriptMenu))]
        public IHttpActionResult DeleteScriptLinkGroup(int id)
        {
            ScriptMenu scriptlinkgroup = db.ScriptMenus.Find(id);
            if (scriptlinkgroup == null)
            {
                return NotFound();
            }

            db.ScriptMenus.Remove(scriptlinkgroup);
            db.SaveChanges();

            return Ok(scriptlinkgroup);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ScriptLinkGroupExists(int id)
        {
            return db.ScriptMenus.Count(e => e.ScriptMenuID == id) > 0;
        }
    }
}