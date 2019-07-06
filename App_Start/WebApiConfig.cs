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

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
            name: "DefaultApi",
            routeTemplate: "api/{controller}/{id}",
            defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
