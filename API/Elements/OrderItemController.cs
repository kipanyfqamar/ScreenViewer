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
    public class OrderItemController : ApiController
    {
        private ScreenPlayEntities db = new ScreenPlayEntities();

        // GET api/OrderItem
        public IQueryable<ScriptItemSelector> GetScriptOrderItems()
        {
            return db.ScriptItemSelectors;
        }

        // GET api/OrderItem/5
        [ResponseType(typeof(ScriptItemSelector))]
        public IHttpActionResult GetScriptOrderItem(int id)
        {
            ScriptItemSelector scriptorderitem = db.ScriptItemSelectors.Find(id);
            if (scriptorderitem == null)
            {
                return NotFound();
            }

            return Ok(scriptorderitem);
        }

        // PUT api/OrderItem/5
        public IHttpActionResult PutScriptOrderItem(int id, ScriptItemSelector scriptorderitem)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != scriptorderitem.ScriptItemSelectorID)
            {
                return BadRequest();
            }

            db.Entry(scriptorderitem).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ScriptOrderItemExists(id))
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

        // POST api/OrderItem
        [ResponseType(typeof(ScriptItemSelector))]
        public IHttpActionResult PostScriptOrderItem(ScriptItemSelector scriptorderitem)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.ScriptItemSelectors.Add(scriptorderitem);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = scriptorderitem.ScriptItemSelectorID }, scriptorderitem);
        }

        // DELETE api/OrderItem/5
        [ResponseType(typeof(ScriptItemSelector))]
        public IHttpActionResult DeleteScriptOrderItem(int id)
        {
            ScriptItemSelector scriptorderitem = db.ScriptItemSelectors.Find(id);
            if (scriptorderitem == null)
            {
                return NotFound();
            }

            db.ScriptItemSelectors.Remove(scriptorderitem);
            db.SaveChanges();

            return Ok(scriptorderitem);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ScriptOrderItemExists(int id)
        {
            return db.ScriptItemSelectors.Count(e => e.ScriptItemSelectorID == id) > 0;
        }
    }
}