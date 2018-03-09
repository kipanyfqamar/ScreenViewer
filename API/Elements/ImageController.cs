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
    public class ImageController : ApiController
    {
        private ScreenPlayEntities db = new ScreenPlayEntities();

        // GET api/Image
        public IQueryable<ScriptImage> GetImages()
        {
            return db.ScriptImages;
        }

        // GET api/Image/5
        [ResponseType(typeof(ScriptImage))]
        public IHttpActionResult GetImage(int id)
        {
            ScriptImage image = db.ScriptImages.Find(id);
            if (image == null)
            {
                return NotFound();
            }

            return Ok(image);
        }

        // PUT api/Image/5
        public IHttpActionResult PutImage(int id, ScriptImage image)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != image.ScriptImageID)
            {
                return BadRequest();
            }

            db.Entry(image).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ImageExists(id))
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

        // POST api/Image
        [ResponseType(typeof(ScriptImage))]
        public IHttpActionResult PostImage(ScriptImage image)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.ScriptImages.Add(image);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = image.ScriptImageID }, image);
        }

        // DELETE api/Image/5
        [ResponseType(typeof(ScriptImage))]
        public IHttpActionResult DeleteImage(int id)
        {
            ScriptImage image = db.ScriptImages.Find(id);
            if (image == null)
            {
                return NotFound();
            }

            db.ScriptImages.Remove(image);
            db.SaveChanges();

            return Ok(image);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ImageExists(int id)
        {
            return db.ScriptImages.Count(e => e.ScriptImageID == id) > 0;
        }
    }
}