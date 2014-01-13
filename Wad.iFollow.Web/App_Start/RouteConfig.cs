﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Wad.iFollow.Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
            name: "ResetPassword",
            url: "Account/NewPassword/{Key}",
            defaults: new { controller = "Account", action = "ResetPassword", Key = UrlParameter.Optional });

            routes.MapRoute(
            name: "MailRegister",
            url: "Account/ConfirmRegistration/{Key}",
            defaults: new { controller = "Account", action = "MailRegister", Key = UrlParameter.Optional });

            routes.MapRoute(
            name: "Wall",
            url: "wall/{action}/{id}",
            defaults: new { controller = "Wall", action = "MainPage", id = UrlParameter.Optional}
            );
    
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
         }
    }
}