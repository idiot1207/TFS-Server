using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using TFSApi.Models;

namespace TFSApi
{
    public class BasicAuthentication : AuthorizationFilterAttribute
    {
        //  public static WebClient client = new WebClient { Credentials = new NetworkCredential("DESKTOP-498RAI\\CGI", "cgi") };

        public static WebClient client;

        public static string UserName;

        public static string Password;

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (actionContext.Request.Headers.Authorization != null)
            {
                var authToken = actionContext.Request.Headers
                    .Authorization.Parameter;

                // decoding authToken we get decode value in 'Username:Password' format  
                var decodeauthToken = System.Text.Encoding.UTF8.GetString(
                    Convert.FromBase64String(authToken));

                // spliting decodeauthToken using ':'   
                var arrUserNameandPassword = decodeauthToken.Split(':');

                // at 0th postion of array we get username and at 1st we get password  
                if (IsAuthorizedUser(arrUserNameandPassword[0], arrUserNameandPassword[1]))
                {
                    // setting current principle  
                    Thread.CurrentPrincipal = new GenericPrincipal(
                    new GenericIdentity(arrUserNameandPassword[0]), null);
                }
                else
                {
                    actionContext.Response = actionContext.Request
                    .CreateResponse(HttpStatusCode.BadRequest);
                }
            }
            else
            {
                actionContext.Response = actionContext.Request
                 .CreateResponse(HttpStatusCode.BadRequest);
            }
        }

        public static bool  IsAuthorizedUser(string Username, string Password)
        {
            // In this method we can handle our database logic here...  
            //return Username == "bhushan" && Password == "demo";
            client = new WebClient { Credentials = new NetworkCredential(Username, Password) };

            try
            {
                var response = client.DownloadString("http://desktop-498rai4:8080/tfs/DefaultCollection/_apis/wit/workItems/40");
            }
            catch(Exception)
            {
                return false;
            }
            return true;

        }
    }
}