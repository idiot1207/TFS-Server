using Newtonsoft.Json;
using System;
using System.Net;
using System.Text;
using System.Web.Http;
using System.Collections.Generic;
using TFSApi.Models;
using TFSApi;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using LoginDecryption;
using System.Linq;

namespace TFSApi.Controllers
{
   // [BasicAuthentication]
   
    public class TFSController : ApiController
    {

        //Base URL For TFS
        string TFSUrl = TreservaUrlHelper.TFSBaseUrl;

        [Route("api/TFS/GetAllTeamName")]
        [Authorize]
        public string GetAllTeamName()
        {
            try
            {

                string teamNameUrl = TreservaUrlHelper.GeteamNameUrl;

                string releaseUrl = TreservaUrlHelper.ReleaseUrl;

                string newTeamNameUrl = string.Format("{0}{1}", TFSUrl, teamNameUrl);

                string newreleaseUrl = String.Format("{0}{1}", TFSUrl, releaseUrl);

                var teamName = BasicAuthentication.client.DownloadString(newTeamNameUrl);

                var release = BasicAuthentication.client.DownloadString(newreleaseUrl);

                var teamAndRelease = "[" + teamName + "," + release + "]";

                return teamAndRelease;
            }
            catch (NullReferenceException)
            {
                return "Please Login";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }

        [Authorize]
        [Route("api/TFS/GetTeamProject/{teamName}")]

        public string GetTeamProject(string teamName)
        {
            try
            {
                // string teamProjectUrl = "ProjectOne/"+teamName+"/_apis/Work/TeamSettings/TeamFieldValues?api-version=2.0";

                string teamProjectUrl = TreservaUrlHelper.TeamProjectUrl;

                string newteamProjectUrl = string.Format("{0}{1}", TFSUrl, teamProjectUrl);

                dynamic projects = JsonConvert.DeserializeObject(BasicAuthentication.client.DownloadString(newteamProjectUrl));

                int counter = 0;

                foreach (var TeamList in projects.value[0].children)
                {
                    foreach (var TeamDatas in TeamList)
                    {
                        foreach (var TeamData in TeamDatas)
                        {

                            if (TeamData.ToString() == teamName || counter != 0)
                            {
                                counter++;

                                var type = TeamData.GetType();

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
            catch (NullReferenceException)
            {
                return "Please Login";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        [HttpPost]

        [Route("api/TFS/login")]

        public async Task<string> login(HttpRequestMessage request)
        {
            try
            {
                var jObject = await request.Content.ReadAsAsync<JObject>();

                var item = JsonConvert.DeserializeObject<userLogin>(jObject.ToString());

                var userName = item.UserName;

                var password = item.Password;

                var decryptedUserName= Logindecryption.DecryptStringAES(userName);

                var decryptedPassword= Logindecryption.DecryptStringAES(password);

                bool isValidUser = BasicAuthentication.IsAuthorizedUser(decryptedUserName, decryptedPassword);

                return isValidUser.ToString();
            }

            catch (Exception e)
            {
                return e.ToString();
            }

        }

        [Authorize]
        [HttpGet]

        [Route("api/TFS/GetReleases/{releaseId}")]

        public string GetReleases(int releaseId)
        {
            try
            {
                string releaseUrl = "ProjectOne/apis/release/releases/" + releaseId + "?api-version=3.0-preview";

                string newreleaseUrl = string.Format("{0}{1}", TFSUrl, releaseUrl);

                var releaseDetails = BasicAuthentication.client.DownloadString(newreleaseUrl);

                return releaseDetails;

            }
            catch (NullReferenceException)
            {
                return "Please Login";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        [Authorize]

        [HttpGet]

        [Route("api/TFS/postRequest/{AreaPath}")]

        public string postRequest(string AreaPath)
        {

            dynamic BP = null;

            int TragetBuisnessPoint = 0;

            int ClosedBuisnessPoint = 0;

            int TargetStoryPoint = 0;

            int ClosedStoryPoint = 0;

            int BugRaised = 0;

            int BugClosed = 0;

            try
            {
                byte[] data = Convert.FromBase64String(AreaPath);

                string decodedString = Encoding.UTF8.GetString(data);

                string uri = TreservaUrlHelper.LINQUrl;

                String[] WorkitemsList = new string[4] { "Feature", "User Stories", "Bug" ,"Task"};

                foreach (string WorkItem in WorkitemsList)
                {
                    BasicAuthentication.client.Headers.Add(HttpRequestHeader.ContentType, "application/json");

                    var query = new { query = "Select [System.Id],[System.Title] from WorkItems where ([System.AreaPath] under '" + decodedString + "' and ([System.WorkItemType] ='" + WorkItem + "'))" };

                  //  var query = new { query = "Select [System.Id],[System.Title] from WorkItems" };


                    var SerializedQuery = JsonConvert.SerializeObject(query);

                    dynamic DeserializedWorkItems = JsonConvert.DeserializeObject(BasicAuthentication.client.UploadString(uri, "POST",SerializedQuery));

                    var newData = JsonConvert.SerializeObject(DeserializedWorkItems);


                    foreach (var item in DeserializedWorkItems.workItems)
                    {

                        string wrokItemUrl = item.url;

                        dynamic DynBP = JsonConvert.DeserializeObject(BasicAuthentication.client.DownloadString(wrokItemUrl));

                        if (WorkItem == "Feature")
                        {

                            BP = DynBP.fields["Microsoft.VSTS.Common.BusinessValue"];

                            TragetBuisnessPoint += Convert.ToInt32(BP);

                            if (DynBP.fields["System.State"] == "Done")
                            {
                                ClosedBuisnessPoint += Convert.ToInt32(BP);
                            }
                        }

                        if (WorkItem == "User Stories")
                        {
                            BP = DynBP.fields["Microsoft.VSTS.Common.BusinessValue"];

                            TargetStoryPoint += Convert.ToInt32(BP);

                            if (DynBP.fields["System.State"] == "Closed")
                            {
                                ClosedStoryPoint += Convert.ToInt32(BP);
                            }
                        }

                        if (WorkItem == "Bug")
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

                ExpectedData.Add("TragetFeaturePoint", TragetBuisnessPoint);

                ExpectedData.Add("ClosedFeaturePoint", ClosedBuisnessPoint);

                ExpectedData.Add("TargetStoryPoint", TargetStoryPoint);

                ExpectedData.Add("ClosedStoryPoint", ClosedStoryPoint);

                ExpectedData.Add("BugRaised", BugRaised);

                ExpectedData.Add("BugClosed", BugClosed);

                var Expectedjson = JsonConvert.SerializeObject(ExpectedData);

                return Expectedjson;
            }
            catch (NullReferenceException)
            {
                return "Please Login";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }










        [Authorize]
        [HttpGet]

        [Route("api/TFS/postRequestNew/{AreaPath}")]

        public string postRequestNew(string AreaPath)

        {

            //AreaPath = "SUJJQ0FPXFRlYW0gU3VtbWVyXElEVyBFa29ub21pc2t0IGJpc3TDpW5kIHJhcHBvcnQ=";

            dynamic BP = null;

            int targetFeaturePoint = 0;

            int closedFeaturePoint = 0;

            int TargetStoryPoint = 0;

            int ClosedStoryPoint = 0;

            int BugRaised = 0;

            int BugClosed = 0;

            byte[] data = Convert.FromBase64String(AreaPath);

            string decodedString = Encoding.UTF8.GetString(data);

            string uri = TreservaUrlHelper.LINQUrl;

            BasicAuthentication.client.Headers.Add(HttpRequestHeader.ContentType, "application/json");

            var query = new { query = "Select [System.Id],[System.Title] from WorkItems where [System.AreaPath] = '" + decodedString + "'" };

            string serializedQuery = JsonConvert.SerializeObject(query);

            //#region Feature Point

            //var fbTargetQuery = new { query = "Select [System.Id],[System.Title] from WorkItems where [System.AreaPath] = '" + decodedString + "' AND [System.WorkItemType] == 'Feature'" };

            //string serializedQuery = JsonConvert.SerializeObject(fbTargetQuery);

            //var fbTargetQueryRes = JsonConvert.DeserializeObject(TreservaUrlHelper.client.UploadString(uri, "POST", serializedQuery));



            ////var etargetFeaturePoint =

            //#endregion

            dynamic DeserializedWorkItems = JsonConvert.DeserializeObject(BasicAuthentication.client.UploadString(uri, "POST", serializedQuery));

            var serializedWorkitems = JsonConvert.SerializeObject(DeserializedWorkItems);

            //List<int> workItemId = serializedWorkitems.



            foreach (var item in DeserializedWorkItems.workItems)

            {

                string wrokItemUrl = item.url;

                dynamic DynBP = JsonConvert.DeserializeObject(BasicAuthentication.client.DownloadString(wrokItemUrl));

                if (DynBP.fields["System.WorkItemType"] == "Feature")

                {

                    BP = DynBP.fields["Microsoft.VSTS.Common.BusinessValue"];

                    targetFeaturePoint += Convert.ToInt32(BP);

                    if (DynBP.fields["System.State"] == "Closed")

                    {

                        closedFeaturePoint += Convert.ToInt32(BP);

                    }

                }

                if (DynBP.fields["System.WorkItemType"] == "User Story")

                {

                    BP = DynBP.fields["Microsoft.VSTS.Scheduling.StoryPoints"];

                    TargetStoryPoint += Convert.ToInt32(BP);

                    if (DynBP.fields["System.State"] == "Closed")

                    {

                        ClosedStoryPoint += Convert.ToInt32(BP);

                    }

                }

                if (DynBP.fields["System.WorkItemType"] == "Bug")

                {

                    BP = 1;

                    BugRaised += Convert.ToInt32(BP);

                    if (DynBP.fields["System.State"] == "Closed")

                    {

                        BugClosed += Convert.ToInt32(BP);

                    }

                }

            }

            Dictionary<string, int> ExpectedData = new Dictionary<string, int>();

            ExpectedData.Add("TargetFeaturePoint", targetFeaturePoint);

            ExpectedData.Add("ClosedFeaturePoint", closedFeaturePoint);

            ExpectedData.Add("TargetStoryPoint", TargetStoryPoint);

            ExpectedData.Add("ClosedStoryPoint", ClosedStoryPoint);

            ExpectedData.Add("BugRaised", BugRaised);

            ExpectedData.Add("BugClosed", BugClosed);

            var Expectedjson = JsonConvert.SerializeObject(ExpectedData);

            return Expectedjson;

        }






        //[Authorize]
        [HttpGet]

        [Route("api/TFS/post/{AreaPath}")]

        public string post(string AreaPath)

        {

            //AreaPath = "SUJJQ0FPXFRlYW0gU3VtbWVyXElEVyBFa29ub21pc2t0IGJpc3TDpW5kIHJhcHBvcnQ=";

            dynamic BP = null;

            int targetFeaturePoint = 0;

            int closedFeaturePoint = 0;

            int TargetStoryPoint = 0;

            int ClosedStoryPoint = 0;

            int BugRaised = 0;

            int BugClosed = 0;

            byte[] data = Convert.FromBase64String(AreaPath);

            string decodedString = Encoding.UTF8.GetString(data);

            string uri = TreservaUrlHelper.LINQUrl;

            String[] workitemsTypeList = new string[4] { "Feature", "User Story", "Bug", "Task" };

            foreach (string workItemType in workitemsTypeList)
            {
                BasicAuthentication.client.Headers.Add(HttpRequestHeader.ContentType, "application/json");

                var query = new { query = "Select [System.Id],[System.Title] from WorkItems where [System.AreaPath] = '" + decodedString + "' and ([System.WorkItemType] ='" + workItemType + "')" };

                string serializedQuery = JsonConvert.SerializeObject(query);

                string json = BasicAuthentication.client.UploadString(uri, "POST", serializedQuery).ToString();

                var DeserializedWorkItems = JsonConvert.DeserializeObject<RootObject>(json);

                var workItemId = DeserializedWorkItems.workItems.Select(p => p.id).ToList();

                int count = workItemId.Count();

                string combindedWorkItemId= string.Join(",", workItemId.ToArray());

                string workItemDetailsUrl = String.Format("{0}{1}", TFSUrl, "_apis/wit/workitems?ids="+combindedWorkItemId+"&api-version=2.0");

                var workItemDetails = BasicAuthentication.client.DownloadString(workItemDetailsUrl);

                var deserializedWorkItemDetails = JsonConvert.DeserializeObject(workItemDetails);

            }

            return "";

        }
    }

    public class RootObject
    {
        public List<WorkItemId> workItems { get; set; }
    }

    public class WorkItemId
    {
        public int id { get; set;}
    }

    public class RootObjectOne
    {
        public List<workItemDetail> value { get; set; }

    }

    public class workItemDetail
    {
        public int targetStoryPoint { get; set; }
        public int closedStoryPoint { get; set; }
        public int targetFeaturePoint{ get; set; }
        public int closedFeaturePoint{ get; set; }
        public int bugRaised { get; set; }
        public int bugClosed { get; set; }
    }
}
