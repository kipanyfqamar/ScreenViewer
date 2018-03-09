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
    public class QuestionareController : ApiController
    {
        private ScreenPlayEntities db = new ScreenPlayEntities();

        // GET api/Questionare
        public IQueryable<ScriptQuestionare> GetScriptQuestionares()
        {
            return db.ScriptQuestionares;
        }

        // GET api/Questionare/5
        [ResponseType(typeof(ScriptQuestionare))]
        public IHttpActionResult GetScriptQuestionare(decimal id)
        {
            ScriptQuestionare scriptquestionare = db.ScriptQuestionares.Find(id);
            if (scriptquestionare == null)
            {
                return NotFound();
            }

            return Ok(scriptquestionare);
        }

        // PUT api/Questionare/5
        public IHttpActionResult PutScriptQuestionare(decimal id, ScriptQuestionare scriptquestionare)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != scriptquestionare.ScriptQuestionareID)
            {
                return BadRequest();
            }

            db.Entry(scriptquestionare).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ScriptQuestionareExists(id))
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

        // POST api/Questionare
        [ResponseType(typeof(ScriptQuestionare))]
        public IHttpActionResult PostScriptQuestionare(ScriptQuestionare scriptquestionare)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.ScriptQuestionares.Add(scriptquestionare);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = scriptquestionare.ScriptQuestionareID }, scriptquestionare);
        }

        // DELETE api/Questionare/5
        [ResponseType(typeof(ScriptQuestionare))]
        public IHttpActionResult DeleteScriptQuestionare(decimal id)
        {
            ScriptQuestionare scriptquestionare = db.ScriptQuestionares.Find(id);
            if (scriptquestionare == null)
            {
                return NotFound();
            }

            db.ScriptQuestionares.Remove(scriptquestionare);
            db.SaveChanges();

            return Ok(scriptquestionare);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ScriptQuestionareExists(decimal id)
        {
            return db.ScriptQuestionares.Count(e => e.ScriptQuestionareID == id) > 0;
        }
    }
}