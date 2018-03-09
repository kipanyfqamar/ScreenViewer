using System;
using System.Collections.Generic;
using System.Globalization;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

using ScreenViewer.Data;

namespace ScreenViewer.API.CRM
{
    public class LogController : ApiController
    {
        private ScreenPlayCRMEntities db = new ScreenPlayCRMEntities();

        // POST api/Project
        [ResponseType(typeof(Data.Log))]
        public IHttpActionResult PostErrorLog(Data.Log log)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                db.Logs.Add(log);
                db.SaveChanges();

                return CreatedAtRoute("DefaultApi", new { id = log.LogId }, log);
            }
            catch (Exception ex)
            {
                return StatusCode(HttpStatusCode.InternalServerError);
            }
        }
    }
}