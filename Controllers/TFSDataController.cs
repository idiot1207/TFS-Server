using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using RestSharp;
using System.Web.Http;

namespace TFSApi.Controllers
{
   
    public class TFSDataController : ApiController
    {
        RestClient a_clinet = new RestClient("http://desktop-498rai4:8080/tfs/DefaultCollection/");

        RestRequest a_request;

        // GET api/<controller>
        public string Get()
        {
            a_request = new RestRequest("_apis/projects/5686e947-ee27-4250-b29c-1cafad947178/teams?api-version=1.0", Method.GET);

            a_request.Credentials = new NetworkCredential(@"DESTOP-498RAI4\CGI", "cgi");

            var a_response = a_clinet.Execute(a_request);

            var TeamName = a_response.Content;

            //return a_response.Content;

            a_request = new RestRequest("ProjectOne/_apis/release/releases?api-version=3.0-preview", Method.GET);

            a_request.Credentials = new NetworkCredential(@"DESTOP-498RAI4\CGI", "cgi");

            a_response = a_clinet.Execute(a_request);

            var ReleaseVersion = a_response.Content;

            string jsonArrayString = "["+ TeamName + ","+ ReleaseVersion + "]";

            return jsonArrayString;


        }

        // GET api/<controller>/5
        public string Get(string TeamName)
        {
            a_request = new RestRequest("ProjectOne/"+TeamName+"/_apis/Work/TeamSettings/TeamFieldValues?api-version=2.0", Method.GET);

            a_request.Credentials = new NetworkCredential(@"DESTOP-498RAI4\CGI", "cgi");

            var a_response = a_clinet.Execute(a_request);

            return a_response.Content;
        }

        // POST api/<controller>
        [HttpGet]
        [Route("api/TFSData/getreleaseDetails/{releaseId}")]
        public string Get(int releaseId)
        {
            a_request = new RestRequest("ProjectOne/_apis/release/releases/" + releaseId + "?api-version=3.0-preview", Method.GET);

            a_request.Credentials = new NetworkCredential(@"DESTOP-498RAI4\CGI", "cgi");

            var a_response = a_clinet.Execute(a_request);

            return a_response.Content;
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody]string value)
        {

        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }
    }
}