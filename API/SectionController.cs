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
    [System.Web.Mvc.Authorize]
    public class SectionController : ApiController
    {
        private ScreenPlayEntities db = new ScreenPlayEntities();

        // GET api/Section
        public IQueryable<ScriptSection> GetScriptSections()
        {
            return db.ScriptSections;
        }

        // GET api/Section/5
        [ResponseType(typeof(ScriptSection))]
        public IHttpActionResult GetScriptSection(decimal id)
        {
            ScriptSection scriptsection = db.ScriptSections.Find(id);
            if (scriptsection == null)
            {
                return NotFound();
            }

            return Ok(scriptsection);
        }

        [ResponseType(typeof(ScriptSection))]
        public IHttpActionResult GetScriptSection(string SectionUniqueName)
        {
            ScriptSection scriptsection = db.ScriptSections.Where(b => b.SectionName.Equals(SectionUniqueName)).FirstOrDefault();

            //ScriptSection scriptsection = db.ScriptSections.Where(b => b.SectionName.Equals(SectionUniqueName)).Include("ScriptSectionLayouts").Select(m => new ScriptSection { ScriptSectionID = m.ScriptSectionID, ClientID = m.ClientID, SectionName = m.SectionName, SectionDesc = m.SectionDesc, DateCreated = m.DateCreated, DateLastModified = m.DateLastModified, UserLastModified = m.UserLastModified, ScriptSectionLayouts = m.ScriptSectionLayouts.OrderBy(o => o.Sequence).ToList() }).FirstOrDefault();

            if (scriptsection == null)
            {
                return NotFound();
            }

            return Ok(scriptsection);
        }

        //[ResponseType(typeof(SectionTemplate))]
        //public IHttpActionResult GetSectionTemplate(string templateName)
        //{
        //    SectionTemplate sectionTemplate = db.SectionTemplates.Where(b => b.TemplateName.Equals(templateName)).FirstOrDefault();
        //    if (sectionTemplate == null)
        //    {
        //        return NotFound();
        //    }

        //    return Ok(sectionTemplate);
        //}

        // Render api/Section/5
        [ResponseType(typeof(String))]
        public IHttpActionResult RenderScriptSection(decimal id)
        {
            ScriptSection scriptsection = db.ScriptSections.Find(id);
            if (scriptsection == null)
            {
                return NotFound();
            }

            return Ok(scriptsection);
        }


        // PUT api/Section/5
        public IHttpActionResult PutScriptSection(decimal id, ScriptSection scriptsection)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != scriptsection.ScriptSectionID)
            {
                return BadRequest();
            }

            db.Entry(scriptsection).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ScriptSectionExists(id))
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

        // POST api/Section
        [ResponseType(typeof(ScriptSection))]
        public IHttpActionResult PostScriptSection(ScriptSection scriptsection)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.ScriptSections.Add(scriptsection);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = scriptsection.ScriptSectionID }, scriptsection);
        }

        // DELETE api/Section/5
        [ResponseType(typeof(ScriptSection))]
        public IHttpActionResult DeleteScriptSection(decimal id)
        {
            ScriptSection scriptsection = db.ScriptSections.Find(id);
            if (scriptsection == null)
            {
                return NotFound();
            }

            db.ScriptSections.Remove(scriptsection);
            db.SaveChanges();

            return Ok(scriptsection);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ScriptSectionExists(decimal id)
        {
            return db.ScriptSections.Count(e => e.ScriptSectionID == id) > 0;
        }

        public string ScriptSectionName(int id)
        {
            return db.ScriptSections.Where(e => e.ScriptSectionID == id).Select(x => x.SectionName).FirstOrDefault();
        }
    }
}