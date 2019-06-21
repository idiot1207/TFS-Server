using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json.Serialization;
using System.Net.Http.Headers;
using System.Web.Http.Cors;

namespace TFSApi
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {

           // var corsAttr = new EnableCorsAttribute("http://localhost:55836/api/TFS/GetAllTeamName", "*", "*");
            config.EnableCors(new EnableCorsAttribute("http://localhost:4200",headers:"*",methods:"*"));//for cross

            
            // Web API configuration and services
            // Configure Web API to use only bearer token authentication.
            config.SuppressDefaultHostAuthentication();
            config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));

            // Web API routes
            config.MapHttpAttributeRoutes();

            //  config.Formatters.JsonFormatter.SupportedMediaTypes.Add(
            //     new MediaTypeHeaderValue("text/html")
            //  );

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{TeamName}",
                defaults: new { TeamName = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
             name: "ActionApi",
             routeTemplate: "api/{controller}/{action}/{resultId}",
             defaults: new { resultId = RouteParameter.Optional }
         );
        }
    }
}
