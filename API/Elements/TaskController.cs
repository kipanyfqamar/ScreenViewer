using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using ScreenViewer.Data;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace ScreenViewer.API.Elements
{
    public class TaskController : ApiController
    {
        private ScreenPlayEntities db = new ScreenPlayEntities();

        public IQueryable<ScriptTask> GetTask()
        {
            return db.ScriptTasks;
        }

        [ResponseType(typeof(ScriptTask))]
        public IHttpActionResult GetTask(int id)
        {
            ScriptTask task = db.ScriptTasks.Find(id);

            if (task == null)
            {
                return NotFound();
            }

            return Ok(task);
        }

    }
}
