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
using System.Data.SqlClient;

namespace ScreenViewer.API
{
    public class ScriptCalculationController : ApiController
    {
        private ScreenPlayEntities db = new ScreenPlayEntities();

        // GET api/ScriptCalculation
        public IQueryable<ScriptCalculation> GetScriptCalculations()
        {
            return db.ScriptCalculations;
        }

        // GET api/ScriptCalculation/5
        [ResponseType(typeof(ScriptCalculation))]
        public IHttpActionResult GetScriptCalculation(decimal id)
        {
            ScriptCalculation scriptcalculation = db.ScriptCalculations.Find(id);
            if (scriptcalculation == null)
            {
                return NotFound();
            }

            return Ok(scriptcalculation);
        }

        // PUT api/ScriptCalculation/5
        public IHttpActionResult PutScriptCalculation(decimal id, ScriptCalculation scriptcalculation)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != scriptcalculation.ScriptCalculationID)
            {
                return BadRequest();
            }

            db.Entry(scriptcalculation).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ScriptCalculationExists(id))
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

        // POST api/ScriptCalculation
        [ResponseType(typeof(ScriptCalculation))]
        public IHttpActionResult PostScriptCalculation(ScriptCalculation scriptcalculation)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.ScriptCalculations.Add(scriptcalculation);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = scriptcalculation.ScriptCalculationID }, scriptcalculation);
        }

        // DELETE api/ScriptCalculation/5
        [ResponseType(typeof(ScriptCalculation))]
        public IHttpActionResult DeleteScriptCalculation(decimal id)
        {
            ScriptCalculation scriptcalculation = db.ScriptCalculations.Find(id);
            if (scriptcalculation == null)
            {
                return NotFound();
            }

            db.ScriptCalculations.Remove(scriptcalculation);
            db.SaveChanges();

            return Ok(scriptcalculation);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ScriptCalculationExists(decimal id)
        {
            return db.ScriptCalculations.Count(e => e.ScriptCalculationID == id) > 0;
        }
    }
}