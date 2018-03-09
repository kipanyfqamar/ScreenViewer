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
    public class ProjectController : ApiController
    {
        private ScreenPlayEntities db = new ScreenPlayEntities();

        // GET api/Project
        public IQueryable<ScriptProject> GetScriptProjects()
        {
            return db.ScriptProjects;
        }

        // GET api/Project/5
        [ResponseType(typeof(ScriptProject))]
        public IHttpActionResult GetScriptProject(int id)
        {
            ScriptProject scriptproject = db.ScriptProjects.Find(id);
            if (scriptproject == null)
            {
                return NotFound();
            }

            return Ok(scriptproject);
        }

        // GET api/Project/5
        [ResponseType(typeof(ScriptProject))]
        public IHttpActionResult GetScriptProject(string id)
        {
            ScriptProject scriptproject = db.ScriptProjects.Where(b => b.ProjectName.Equals(id)).FirstOrDefault();
            if (scriptproject == null)
            {
                return NotFound();
            }

            return Ok(scriptproject);
        }

        public int GetProjectId(string name)
        {
            try
            {
                return db.ScriptProjects.Where(b => b.ProjectName.Equals(name)).FirstOrDefault().ScriptProjectID;
            }
            catch (Exception ex)
            {
                throw new System.Web.HttpException(404, "Not found");
            }
        }

        [ResponseType(typeof(ScriptImage))]
        public IHttpActionResult GetScriptProjectImage(int id)
        {
            ScriptImage image = db.ScriptImages.Find(id);
            if (image == null)
            {
                return NotFound();
            }

            return Ok(image);
        }

        [ResponseType(typeof(ScriptMenu))]
        public IHttpActionResult GetScriptProjectMenu(int id)
        {
            ScriptMenu scriptMenu = db.ScriptMenus.Find(id);

            if (scriptMenu == null)
            {
                return NotFound();
            }

            return Ok(scriptMenu);
        }

        // PUT api/Project/5
        public IHttpActionResult PutScriptProject(int id, ScriptProject scriptproject)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != scriptproject.ScriptProjectID)
            {
                return BadRequest();
            }

            db.Entry(scriptproject).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ScriptProjectExists(id))
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

        // POST api/Project
        [ResponseType(typeof(ScriptProject))]
        public IHttpActionResult PostScriptProject(ScriptProject scriptproject)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.ScriptProjects.Add(scriptproject);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = scriptproject.ScriptProjectID }, scriptproject);
        }

        // DELETE api/Project/5
        [ResponseType(typeof(ScriptProject))]
        public IHttpActionResult DeleteScriptProject(int id)
        {
            ScriptProject scriptproject = db.ScriptProjects.Find(id);
            if (scriptproject == null)
            {
                return NotFound();
            }

            db.ScriptProjects.Remove(scriptproject);
            db.SaveChanges();

            return Ok(scriptproject);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ScriptProjectExists(int id)
        {
            return db.ScriptProjects.Count(e => e.ScriptProjectID == id) > 0;
        }

    }
}