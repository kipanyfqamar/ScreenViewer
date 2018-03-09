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
using System.Web.Http.Results;

namespace ScreenViewer.API
{
    public class ItemController : ApiController
    {
        private ScreenPlayEntities db = new ScreenPlayEntities();

        // GET api/Item
        public IQueryable<ScriptItem> GetScriptItems()
        {
            return db.ScriptItems;
        }

        // GET api/Item/5
        [ResponseType(typeof(ScriptItem))]
        public IHttpActionResult GetScriptItem(string id)
        {
            ScriptItem scriptitem = db.ScriptItems.Find(id);
            if (scriptitem == null)
            {
                return NotFound();
            }

            return Ok(scriptitem);
        }

        // GET api/Item/5
        [ResponseType(typeof(ScriptItem))]
        public IHttpActionResult GetScriptItemByCode(string code)
        {
            ScriptItem scriptitem = db.ScriptItems.Where(x=> x.ItemCode == code).FirstOrDefault();
            if (scriptitem == null)
            {
                return NotFound();
            }

            return Ok(scriptitem);
        }


        public ScriptItem GetActiveScriptItem(string Category, string SubCategory, string id)
        {
            return db.ScriptItems.Where(b => b.ItemCategory.Equals(Category) && b.ItemSubCategory.Equals(SubCategory) && b.ItemCode==id && b.Active == true).FirstOrDefault();

        }

        public ScriptItem GetScriptItem(string Category, string SubCategory, string id)
        {
            return db.ScriptItems.Where(b => b.ItemCategory.Equals(Category) && b.ItemSubCategory.Equals(SubCategory) && b.ItemCode == id).FirstOrDefault();

        }

        public IQueryable<ScriptItem> GetActiveScriptItems(string Category)
        {
            return db.ScriptItems.Where(b => b.ItemCategory.Equals(Category) && b.Active == true);

        }


        public IQueryable<ScriptItem> GetActiveScriptItems(string Category, string SubCategory)
        {
            return db.ScriptItems.Where(b => b.ItemCategory.Equals(Category) && b.ItemSubCategory.Equals(SubCategory) && b.Active == true);
         }
        
        // PUT api/Item/5
        public IHttpActionResult PutScriptItem(string id, ScriptItem scriptitem)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != scriptitem.ItemCode)
            {
                return BadRequest();
            }

            db.Entry(scriptitem).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ScriptItemExists(id))
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

        // POST api/Item
        [ResponseType(typeof(ScriptItem))]
        public IHttpActionResult PostScriptItem(ScriptItem scriptitem)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.ScriptItems.Add(scriptitem);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException)
            {
                if (ScriptItemExists(scriptitem.ItemCode))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = scriptitem.ItemCode }, scriptitem);
        }

        // DELETE api/Item/5
        [ResponseType(typeof(ScriptItem))]
        public IHttpActionResult DeleteScriptItem(string id)
        {
            ScriptItem scriptitem = db.ScriptItems.Find(id);
            if (scriptitem == null)
            {
                return NotFound();
            }

            db.ScriptItems.Remove(scriptitem);
            db.SaveChanges();

            return Ok(scriptitem);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ScriptItemExists(string id)
        {
            return db.ScriptItems.Count(e => e.ItemCode == id) > 0;
        }

        public List<ScreenViewer.Models.KeyViewModel> FilterKeyList(List<string> keys)
        {
            var rows = (from c in db.ScriptItemKeys
                         where keys.Contains(c.ItemKey)
                         select new ScreenViewer.Models.KeyViewModel
                         {
                             Key = c.ItemKey,
                             Value = c.ItemValue
                         }).Distinct().ToList();


            return rows;
        }
    }
}