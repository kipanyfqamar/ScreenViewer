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
    public class TextHTMLController : ApiController
    {
        private ScreenPlayEntities db = new ScreenPlayEntities();

        // GET api/TextHTML
        public IQueryable<ScriptTextHTML> GetScriptTextHTMLs()
        {
            return db.ScriptTextHTMLs;
        }


        [ResponseType(typeof(List<KBSearch_Result>))]
        public IHttpActionResult GetKBSearchResults(string SearchPhrase,string ClientID)
        {
            List<KBSearch_Result> theresult = null;
            theresult = db.KBSearch(SearchPhrase,ClientID).ToList();

            return Ok(theresult);
        }



        // GET api/TextHTML/5
        [ResponseType(typeof(ScriptTextHTML))]
        public IHttpActionResult GetScriptTextHTML(decimal id)
        {
            ScriptTextHTML scripttexthtml = db.ScriptTextHTMLs.Find(id);
            if (scripttexthtml == null)
            {
                return NotFound();
            }

            return Ok(scripttexthtml);
        }



        [ResponseType(typeof(ScriptHTML))]
        public IHttpActionResult GetScriptHTML(decimal id)
        {
            ScriptHTML scriptHTML = db.ScriptHTMLs.Find(id);
            if (scriptHTML == null)
            {
                return NotFound();
            }

            return Ok(scriptHTML);
        }

        [ResponseType(typeof(String))]
        public IHttpActionResult RenderScriptTextHTML(decimal id)
        {
            ScriptTextHTML STH = db.ScriptTextHTMLs.Find(id);
            if (STH == null)
            {
                return NotFound();
            }


            //if ((bool)STH.ShowDescTooltip)
            //{

                
            //    label = new Label();
            //    label.ID = "PUT_" + STH.TextHTMLID;
            //    label.BorderStyle = BorderStyle.Dotted;
            //    label.BorderWidth = new Unit(1);

            //    label.BorderColor = System.Drawing.Color.Red;
            //    if (STH.Description == "")
            //    {
            //        label.Text = "Hover mouse here";
            //    }
            //    else
            //    {
            //        label.Text = STH.Description;
            //    }
            //    RadToolTip RTT = new RadToolTip();
            //    RTT.TargetControlID = label.ID;
            //    RTT.Text = ReplaceObjectsandQuestions(STH.Content);
            //    RTT.ID = "RTT_" + STH.TextHTMLID;
            //    //label.Text = ReplaceObjectsandQuestions(STH.Content);

            //    label.CssClass = STH.StyleGroup;

            //    RTT.AutoCloseDelay = 60000;
            //    RTT.Position = ToolTipPosition.BottomCenter;
            //    RTT.Width = System.Web.UI.WebControls.Unit.Parse(this.ScriptWidth);
            //    RTT.ShowDelay = 500;
            //    RTT.RelativeTo = ToolTipRelativeDisplay.Element;
            //    RTT.Modal = true;
            //    RTT.HideEvent = ToolTipHideEvent.LeaveToolTip;


            //    //Droptocontrols.Controls.Add(CreateEditLink(ScrEl));
            //    Droptocontrols.Controls.Add(label);
            //    Droptocontrols.Controls.Add(RTT);
            //    Droptocontrols.Controls.Add(new LiteralControl("<br>"));


           // }
           // else
           // {
                StringWriter stringwriter = new StringWriter();
                HtmlTextWriter HTW = new HtmlTextWriter(stringwriter);
 
                Label label = new Label();
                //label.Text = ReplaceObjectsandQuestions(STH.TextHTMLContent);
                label.Text = STH.TextHTMLContent;
                //label.CssClass = STH.StyleGroup;
                label.RenderControl(HTW);
               
                return Ok(stringwriter.ToString());
           // }


            ////Droptocontrols.Controls.Add(new LiteralControl("<br>"));
            //HasStuffToDisplay = true;
            //break;


            //return Ok("");
        }




        // PUT api/TextHTML/5
        public IHttpActionResult PutScriptTextHTML(decimal id, ScriptTextHTML scripttexthtml)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != scripttexthtml.ScriptTextHTMLID)
            {
                return BadRequest();
            }

            db.Entry(scripttexthtml).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ScriptTextHTMLExists(id))
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

        // POST api/TextHTML
        [ResponseType(typeof(ScriptTextHTML))]
        public IHttpActionResult PostScriptTextHTML(ScriptTextHTML scripttexthtml)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.ScriptTextHTMLs.Add(scripttexthtml);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = scripttexthtml.ScriptTextHTMLID }, scripttexthtml);
        }

        // DELETE api/TextHTML/5
        [ResponseType(typeof(ScriptTextHTML))]
        public IHttpActionResult DeleteScriptTextHTML(decimal id)
        {
            ScriptTextHTML scripttexthtml = db.ScriptTextHTMLs.Find(id);
            if (scripttexthtml == null)
            {
                return NotFound();
            }

            db.ScriptTextHTMLs.Remove(scripttexthtml);
            db.SaveChanges();

            return Ok(scripttexthtml);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ScriptTextHTMLExists(decimal id)
        {
            return db.ScriptTextHTMLs.Count(e => e.ScriptTextHTMLID == id) > 0;
        }
    }
}