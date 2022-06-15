using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace WebApplication2
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");


            routes.MapRoute(
                name: "Route1",
                url: "{controller}/{action}/{*catchall}",
                defaults: new { catchall = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Default",
                url: "{*catchall}",
                defaults: new { controller = "Home", action = "Redirect", catchall = UrlParameter.Optional }
            );
        }
    }
}
