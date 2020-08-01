using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace WebGUI
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            // http://www.codedigest.com/posts/3/adding-aspnet-webforms-into-aspnet-mvc-project-and-vice-versa
            // Additionally added to handle ASPX PAGES
            routes.RouteExistingFiles = true;
            //routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            //routes.IgnoreRoute("Content/{*pathInfo}");
            //routes.IgnoreRoute("Scripts/{*pathInfo}");
            routes.IgnoreRoute("{WebPage}.aspx/{*pathInfo}");


            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );


            // https://weblogs.asp.net/scottgu/url-routing-with-asp-net-4-web-forms-vs-2010-and-net-4-0-series
            // Adding routes for WEB forms page
            routes.MapPageRoute(
                "RadViewUCRoute",
                "RadView",
                "~/RadUCWebForm.aspx"
                );
        }
    }
}
