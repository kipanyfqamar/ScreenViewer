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
    public class QuestionController : ApiController
    {
        private ScreenPlayEntities db = new ScreenPlayEntities();

        // GET api/Question
        public IQueryable<ScriptQuestion> GetScriptQuestions()
        {
            return db.ScriptQuestions;
        }

        // GET api/Question/5
        [ResponseType(typeof(ScriptQuestion))]
        public IHttpActionResult GetScriptQuestion(decimal id)
        {
            ScriptQuestion scriptquestion = db.ScriptQuestions.Find(id);
            if (scriptquestion == null)
            {
                return NotFound();
            }

            return Ok(scriptquestion);
        }

        [ResponseType(typeof(ScriptLookup))]
        public IHttpActionResult GetScriptQuestionLookup(decimal id)
        {
            ScriptLookup scriptLookup = db.ScriptLookups.Find(id);
            if (scriptLookup == null)
            {
                return NotFound();
            }

            return Ok(scriptLookup);
        }

        [ResponseType(typeof(ScriptValidator))]
        public IHttpActionResult GetScriptTextValidator(decimal id)
        {
            ScriptValidator scriptTextValidator = db.ScriptValidators.Find(id);
            if (scriptTextValidator == null)
            {
                return NotFound();
            }

            return Ok(scriptTextValidator);
        }

        // PUT api/Question/5
        public IHttpActionResult PutScriptQuestion(decimal id, ScriptQuestion scriptquestion)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != scriptquestion.ScriptQuestionID)
            {
                return BadRequest();
            }

            db.Entry(scriptquestion).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ScriptQuestionExists(id))
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

        // POST api/Question
        [ResponseType(typeof(ScriptQuestion))]
        public IHttpActionResult PostScriptQuestion(ScriptQuestion scriptquestion)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.ScriptQuestions.Add(scriptquestion);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = scriptquestion.ScriptQuestionID }, scriptquestion);
        }

        // DELETE api/Question/5
        [ResponseType(typeof(ScriptQuestion))]
        public IHttpActionResult DeleteScriptQuestion(decimal id)
        {
            ScriptQuestion scriptquestion = db.ScriptQuestions.Find(id);
            if (scriptquestion == null)
            {
                return NotFound();
            }

            db.ScriptQuestions.Remove(scriptquestion);
            db.SaveChanges();

            return Ok(scriptquestion);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ScriptQuestionExists(decimal id)
        {
            return db.ScriptQuestions.Count(e => e.ScriptQuestionID == id) > 0;
        }
    }
}