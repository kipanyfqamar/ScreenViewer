using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using ScreenViewer.Data;

namespace ScreenViewer.API
{
    public class VariableController : ApiController
    {
        private ScreenPlayEntities db = new ScreenPlayEntities();

        public IQueryable<ScriptVariable> GetScriptVariable()
        {
            return db.ScriptVariables;
        }

        [ResponseType(typeof(ScriptVariable))]
        public IHttpActionResult GetScriptVariable(int id)
        {
            ScriptVariable scriptVariable = db.ScriptVariables.Find(id);
            if (scriptVariable == null)
            {
                return NotFound();
            }

            return Ok(scriptVariable);
        }

        [ResponseType(typeof(ScriptVariable))]
        public IHttpActionResult GetScriptVariable(string name)
        {
            ScriptVariable scriptVariable = db.ScriptVariables.Where(b => b.VariableDesc.Equals(name)).FirstOrDefault();

            if (scriptVariable == null)
            {
                return NotFound();
            }

            return Ok(scriptVariable);
        }

        public string GetVariableName(int id)
        {
            try
            {
                return db.ScriptVariables.Where(b => b.ScriptVariableID.Equals(id)).FirstOrDefault().VariableDesc;
            }
            catch
            {
                throw new System.Web.HttpException(404, "Not found");
            }
        }
    }
}
