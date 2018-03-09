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
using ScreenViewer.Models;

namespace ScreenViewer.API
{
    public class ClauseController : ApiController
    {
        private ScreenPlayEntities db = new ScreenPlayEntities();

        // GET api/Clause
        public IQueryable<Data.ScriptClause> GetScriptClauses()
        {
            return db.ScriptClauses;
        }

        // GET api/Clause/5
        public Data.ScriptClause GetScriptClaus(int id)
        {
            Data.ScriptClause scriptclaus = db.ScriptClauses.Find(id);
            if (scriptclaus == null)
            {
                return null;
            }

            return scriptclaus;
        }

        // GET api/Clause/5
        [ResponseType(typeof(Clause))]
        public IHttpActionResult GetClause(int id)
        {
            Data.ScriptClause scriptclause = GetScriptClaus(id);
            

            if (scriptclause == null)
            {
                return NotFound();
            }

            Clause theClause = Clause.CreateClauseFromXML(scriptclause.ClauseXML);


            return Ok(theClause);
        }


        private Clause GetClauseLocal(int id)
        {
            Data.ScriptClause scriptclause = GetScriptClaus(id);


            if (scriptclause == null)
            {
                return null;
            }

            Clause theClause = Clause.CreateClauseFromXML(scriptclause.ClauseXML);


            return theClause;
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ScriptClausExists(int id)
        {
            return db.ScriptClauses.Count(e => e.ScriptClauseID == id) > 0;
        }

        public string GetClauseResult(Int32 TheClauseID)
        {
            Clause TheClause = GetClauseLocal(TheClauseID);
            if (TheClause == null)
            {
                return "";
            }
            return GetClauseResult(TheClause, "");


        }


        public bool GetClauseItemResult(Int32 TheClauseID)
        {
            Clause TheClause = GetClauseLocal(TheClauseID);
            if (TheClause == null)
            {
                return false;
            }
            return GetClauseItemResult(TheClause);


        }
        protected bool GetClauseItemResult(Clause TheClause)
        {


            if (TheClause == null)
            {

                return false;
            }

            if (TheClause.SubClause != null)
            {
                foreach (Clause claws in TheClause.SubClause)
                {
                    if (claws.SubClause != null)
                    {

                        bool critty = GetClauseItemResult(claws);
                        if (critty)
                        {
                            return critty;
                        }
                    }
                    else if (claws.LSide.EType == Clause.ElementTypeLeft.Clause)
                    {
                        Clause SC = GetClauseLocal(Convert.ToInt32(claws.LSide.CID));
                        bool critty2 = GetClauseItemResult(SC);
                        if (critty2)
                        {
                            return critty2;
                        }
                    }
                    else
                    {
                        return CheckForItems(claws);

                    }
                }
            }
            else
            {
                if (TheClause.LSide.EType == Clause.ElementTypeLeft.Clause)
                {
                    Clause SC = GetClauseLocal(Convert.ToInt32(TheClause.LSide.CID));
                    bool critty3 = GetClauseItemResult(SC);
                    if (critty3)
                    {
                        return critty3;

                    }

                }

            }


            return false;
        }


        protected string GetClauseResult(Clause TheClause, string theclauses)
        {

            if (theclauses == "")
            {
                theclauses = "*";
            }

            if (TheClause == null)
            {

                return theclauses;
            }

            if (TheClause.SubClause != null)
            {
                foreach (Clause claws in TheClause.SubClause)
                {
                    if (claws.SubClause != null)
                    {

                        theclauses = GetClauseResult(claws, theclauses);
                    }
                    else if (claws.LSide.EType == Clause.ElementTypeLeft.Clause)
                    {
                        Clause SC = GetClauseLocal(Convert.ToInt32(claws.LSide.CID));
                        theclauses = GetClauseResult(SC, theclauses);

                    }
                    else
                    {
                        theclauses += CheckForQuestions(claws);

                    }
                }
            }
            else
            {
                if (TheClause.LSide.EType == Clause.ElementTypeLeft.Clause)
                {
                    Clause SC = GetClauseLocal(Convert.ToInt32(TheClause.LSide.CID));
                    theclauses = GetClauseResult(SC, theclauses);

                }

            }




            return theclauses;
        }

        public static string CheckForQuestions(Clause checkclause)
        {
            DateTime leftdate = DateTime.Now;
            LExpr LE = checkclause.LSide;
            string retquest = "";
            if (LE.EType == Clause.ElementTypeLeft.Question)
            {
                retquest += "," + LE.Qid.ToString();
            }


            RExpr RE = checkclause.RSide;

            if (RE.EType == Clause.ElementTypeRight.Question)
            {
                retquest += "," + RE.Qid.ToString();
            }


            return retquest;

        }
        public static bool CheckForItems(Clause checkclause)
        {
            DateTime leftdate = DateTime.Now;
            LExpr LE = checkclause.LSide;
            string retquest = "";
            if (LE.EType == Clause.ElementTypeLeft.Item || LE.EType == Clause.ElementTypeLeft.Item )
            {
                return true;
            }


            RExpr RE = checkclause.RSide;

            if (RE.EType == Clause.ElementTypeRight.Item || RE.EType == Clause.ElementTypeRight.Item)
            {
                return true;
            }


            return false;

        }
    }
}