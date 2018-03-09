using System;

using System.Data;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Http.Results;
using ScreenViewer.API;
using ScreenViewer.Models;
using System.IO;
using System.Text;
using ScreenViewer.Models.Elements;
using Kendo.Mvc;
using Kendo.Mvc.UI;

namespace ScreenViewer.Controllers
{
    public class LinkGroupController : Controller
    {
        //
        // GET: /LinkGroup/
        public ActionResult Index()
        {
            return View();
        }

        public string Render(int ID, ControllerContext ContCont)
        {
            ScreenViewer.API.ProjectController PC = new API.ProjectController();
            var scriptMenu = PC.GetScriptProjectMenu(ID);
            var menuResponse = scriptMenu as OkNegotiatedContentResult<Data.ScriptMenu>;
            Data.ScriptMenu theLinkGroup = menuResponse.Content;

            DataTable menuTable = new DataTable();
            menuTable.Columns.Add("Item", typeof(string));
            menuTable.Columns.Add("Url", typeof(string));
            menuTable.Columns.Add("Window", typeof(string));

            using (StringReader sr = new StringReader(menuResponse.Content.MenuXML))
            {
                XmlSerializer xs = new XmlSerializer(typeof(ScriptUL));
                ScriptUL ul = (ScriptUL)xs.Deserialize(sr);
                List<MenuItem> lstMenuItem = new List<MenuItem>();
                foreach (ScriptLI li in ul.LIArray)
                {
                    lstMenuItem.Add(PopulateMenuItem(li));
                }

                ViewBag.LinkGroup = lstMenuItem;
                ViewBag.Orientation = theLinkGroup.MenuOrientation;
            }

            return RenderHelper.RenderViewToString(ContCont, "~/Views/LinkGroup/_LinkGroup.cshtml", theLinkGroup, ViewData);

        }

        private MenuItem PopulateMenuItem(ScriptLI li)
        {
            MenuItem item = new MenuItem();

            if (!li.IsListItem)
            {
                item.Text = li.Text;
            }
            else
            {
                ScreenViewer.API.Elements.LinkController controller = new ScreenViewer.API.Elements.LinkController();
                var scriptLink = controller.GetScriptLink(Convert.ToDecimal(li.ElementID));
                var linkResponse = scriptLink as OkNegotiatedContentResult<Data.ScriptLink>;

                if (linkResponse.Content.LinkType == "Web")
                {
                    item.Text = linkResponse.Content.LinkDesc;
                    item.Url = linkResponse.Content.LinkURL;

                    if (linkResponse.Content.LinkNewWindow.Equals(true))
                    {
                        item.LinkHtmlAttributes.Add("target", "_blank");
                    }

                }

                if (linkResponse.Content.LinkType == "Workflow")
                {
                    item.Text = linkResponse.Content.LinkDesc;
                    item.Action("DisplayByUniqueName", "Workflow", new { id = linkResponse.Content.LinkTypeID });

                    if (linkResponse.Content.LinkNewWindow.Equals(true))
                        item.HtmlAttributes.Add("target", "_blank");

                    //item.HtmlAttributes.Add("onclick", "$('#myForm #btnSave').click();");
                }

                if (linkResponse.Content.LinkType == "Section")
                {
                    item.Text = linkResponse.Content.LinkDesc;
                    item.Action("DisplayByUniqueName", "Section", new { id = linkResponse.Content.LinkTypeID });

                    if (linkResponse.Content.LinkNewWindow.Equals(true))
                        item.HtmlAttributes.Add("target", "_blank");

                }
            }

            if (li.theUL != null && li.theUL.LIArray.Length > 0)
            {
                foreach (ScriptLI cli in li.theUL.LIArray)
                {
                    item.Items.Add(PopulateMenuItem(cli));
                }
            }

            return item;
        }
	}
}