using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace ScreenViewer
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

           config.Routes.MapHttpRoute(
           name: "APIwithDateRange",
           routeTemplate: "API/{controller}/GetRangeContacts/{startdate}/{enddate}/{VendorCode}"
           );

            config.Routes.MapHttpRoute(
            name: "APIwithActionVendor",
            routeTemplate: "API/{controller}/{action}/{id}"
            );


            config.Routes.MapHttpRoute(
            name: "APIwithActionIDVendor",
            routeTemplate: "API/{controller}/{action}/{id}/{VendorCode}"
            );

            config.Routes.MapHttpRoute(
            name: "APIwithActionnoparam",
            routeTemplate: "API/{controller}/{action}"
            );
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
