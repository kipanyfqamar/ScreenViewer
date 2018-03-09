using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace ScreenViewer
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "project",
                url: "{projectname}",
                defaults: new { controller = "Project", action = "DisplayProject", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "KeywordSearch",
                url: "TextHTML/SearchKnowledgeBase/{SearchPhrase}",
                defaults: new { controller = "TextHTML", action = "SearchKnowledgeBase", SearchPhrase = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "dashboard",
                url: "Dashboard/{dashboardname}",
                defaults: new { controller = "Dashboard", action = "DisplayDashboard", dashboardname = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "dashboarda",
                url: "Dashboard/myForm/{id}",
                defaults: new { controller = "Dashboard", action = "myForm", id = UrlParameter.Optional }
            );


            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DefaultWithLayout",
                url: "{controller}/{action}/{id}/{layoutId}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional, layoutId = UrlParameter.Optional }
            );

        }
    }
}
