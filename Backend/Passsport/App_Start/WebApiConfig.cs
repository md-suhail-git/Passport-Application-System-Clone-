
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;

namespace Passsport
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            var cors = new EnableCorsAttribute(
   origins: "http://localhost:4200",
   headers: "*",
   methods: "*"
);
            config.EnableCors(cors);

            config.MapHttpAttributeRoutes();

            config.Filters.Add(new AuthorizeAttribute());


            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
