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
    public class DataViewController : ApiController
    {
        private ScreenPlayEntities db = new ScreenPlayEntities();

        // GET api/DataView
        public IQueryable<ScriptDataView> GetScriptDataViews()
        {
            return db.ScriptDataViews;
        }

        // GET api/DataView/5
        [ResponseType(typeof(ScriptDataView))]
        public IHttpActionResult GetScriptDataView(decimal id)
        {
            ScriptDataView scriptdataview = db.ScriptDataViews.Find(id);
            if (scriptdataview == null)
            {
                return NotFound();
            }

            return Ok(scriptdataview);
        }

        // PUT api/DataView/5
        public IHttpActionResult PutScriptDataView(decimal id, ScriptDataView scriptdataview)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != scriptdataview.ScriptDataViewID)
            {
                return BadRequest();
            }

            db.Entry(scriptdataview).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ScriptDataViewExists(id))
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

        // POST api/DataView
        [ResponseType(typeof(ScriptDataView))]
        public IHttpActionResult PostScriptDataView(ScriptDataView scriptdataview)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.ScriptDataViews.Add(scriptdataview);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = scriptdataview.ScriptDataViewID }, scriptdataview);
        }

        // DELETE api/DataView/5
        [ResponseType(typeof(ScriptDataView))]
        public IHttpActionResult DeleteScriptDataView(decimal id)
        {
            ScriptDataView scriptdataview = db.ScriptDataViews.Find(id);
            if (scriptdataview == null)
            {
                return NotFound();
            }

            db.ScriptDataViews.Remove(scriptdataview);
            db.SaveChanges();

            return Ok(scriptdataview);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ScriptDataViewExists(decimal id)
        {
            return db.ScriptDataViews.Count(e => e.ScriptDataViewID == id) > 0;
        }
    }
}