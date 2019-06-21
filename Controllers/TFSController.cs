using Newtonsoft.Json;
using System;
using System.Net;
using System.Text;
using System.Web.Http;
using System.Collections.Generic;
using TFSApi.Models;

namespace TFSApi.Controllers
{
    public class TFSController : ApiController
    {
        //Credentials 
        WebClient client = new WebClient { Credentials = new NetworkCredential("DESKTOP-498RAI\\CGI", "cgi") };

        //Base URL For TFS
        string TFSUrl = UrlHelper.TFSBaseUrl;

        [Route("api/TFS/GetAllTeamName")]

        public string GetAllTeamName()
        {
            try
            {

                string teamNameUrl = UrlHelper.GeteamNameUrl;

                string releaseUrl = UrlHelper.ReleaseUrl;

                string newTeamNameUrl = string.Format("{0}{1}", TFSUrl, teamNameUrl);

                string newreleaseUrl = String.Format("{0}{1}", TFSUrl, releaseUrl);

                var teamName = client.DownloadString(newTeamNameUrl);

                var release = client.DownloadString(newreleaseUrl);

                var teamAndRelease = "[" + teamName + "," + release + "]";

                return teamAndRelease;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        [Route("api/TFS/GetTeamProject/{teamName}")]

        public string GetTeamProject(string teamName)
        {
            try
            {
                // string teamProjectUrl = "ProjectOne/"+teamName+"/_apis/Work/TeamSettings/TeamFieldValues?api-version=2.0";

                string teamProjectUrl = UrlHelper.TeamProjectUrl;

                string newteamProjectUrl = string.Format("{0}{1}", TFSUrl, teamProjectUrl);

                dynamic projects  = JsonConvert.DeserializeObject(client.DownloadString(newteamProjectUrl));

                int counter  = 0;

                foreach (var TeamList in projects.value[0].children)
                {
                    foreach(var TeamDatas in TeamList)
                    {
                       foreach(var TeamData in TeamDatas)
                        {
                            
                            if(TeamData.ToString() == teamName || counter != 0)
                            {
                                counter++;
                               
                                var type= TeamData.GetType();

                                var pros = type.ToString();

                                if (pros == "Newtonsoft.Json.Linq.JArray")
                                {
                                    projects = TeamData.ToString();
                                }  
                                
                            }
                          
                        }

                    }
                    counter = 0;
                }

                return projects;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        [HttpGet]

        [Route("api/TFS/login/{userName}/{password}")]

        public string login(string userName, string password)
        {
          //  client = new WebClient { Credentials = new NetworkCredential(userName, password) };

            try
            {
                client = new WebClient { Credentials = new NetworkCredential(userName, password) };

                return "Login Successfull";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        [HttpGet]

        [Route("api/TFS/GetReleases/{releaseId}")]

        public string GetReleases(int releaseId)
        {
            try
            {
                string releaseUrl = "ProjectOne/apis/release/releases/" + releaseId + "?api-version=3.0-preview";

                string newreleaseUrl = string.Format("{0}{1}", TFSUrl, releaseUrl);

                var releaseDetails = client.DownloadString(newreleaseUrl);

                return releaseDetails;

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        [HttpGet]

        [Route("api/TFS/postRequest/{AreaPath}")]

        public string postRequest(string AreaPath)
        {

            dynamic BP=null;

            int TragetBuisnessPoint = 0;

            int ClosedBuisnessPoint = 0;

            int TargetStoryPoint = 0;

            int ClosedStoryPoint = 0;

            int BugRaised = 0;

            int BugClosed = 0;


            byte[] data = Convert.FromBase64String(AreaPath);

            string decodedString = Encoding.UTF8.GetString(data);

            string uri = UrlHelper.LINQUrl;

            String[] WorkitemsList = new string[3] { "Feature", "User Stories", "Bug" };

            foreach(string WorkItem in WorkitemsList)
            {
                client.Headers.Add(HttpRequestHeader.ContentType, "application/json");

                var query = new { query = "Select [System.Id],[System.Title] from WorkItems where ([System.AreaPath] under '" + decodedString + "' and ([System.WorkItemType] ='"+ WorkItem + "'))" };

                var SerializedQuery = JsonConvert.SerializeObject(query);

                dynamic DeserializedWorkItems = JsonConvert.DeserializeObject(client.UploadString(uri, "POST", SerializedQuery));

                foreach (var item in DeserializedWorkItems.workItems)
                {

                    string wrokItemUrl = item.url;

                    dynamic DynBP = JsonConvert.DeserializeObject(client.DownloadString(wrokItemUrl));

                    if (WorkItem == "Feature")
                    {

                        BP = DynBP.fields["Microsoft.VSTS.Common.BusinessValue"];

                        TragetBuisnessPoint += Convert.ToInt32(BP);

                        if (DynBP.fields["System.State"] == "Done")
                        {
                            ClosedBuisnessPoint += Convert.ToInt32(BP);
                        }
                    }

                    if(WorkItem == "User Stories")
                    {
                        BP = DynBP.fields["Microsoft.VSTS.Common.BusinessValue"];

                        TargetStoryPoint += Convert.ToInt32(BP);

                        if (DynBP.fields["System.State"] == "Closed")
                        {
                            ClosedStoryPoint += Convert.ToInt32(BP);
                        }
                    }

                    if(WorkItem == "Bug")
                    {
                        BP = 1;

                        BugRaised += Convert.ToInt32(BP);

                        if (DynBP.fields["System.State"] == "Done")
                        {
                            BugClosed += Convert.ToInt32(BP);
                        }
                    }
                }

            }

            Dictionary<string, int> ExpectedData = new Dictionary<string, int>();

            ExpectedData.Add("TragetFeraturePoint", TragetBuisnessPoint);

            ExpectedData.Add("ClosedFeraturePoint", ClosedBuisnessPoint);

            ExpectedData.Add("TargetStoryPoint", TargetStoryPoint);

            ExpectedData.Add("ClosedStoryPoint", ClosedStoryPoint);

            ExpectedData.Add("BugRaised", BugRaised);

            ExpectedData.Add("BugClosed", BugClosed);

            var Expectedjson = JsonConvert.SerializeObject(ExpectedData);

            return Expectedjson;
           
        }
    }
}
