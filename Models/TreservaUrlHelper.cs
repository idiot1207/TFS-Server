using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TFSApi.Models
{
    public class TreservaUrlHelper
    {
        public static string TFSBaseUrl = "http://desktop-498rai4:8080/tfs/DefaultCollection/";

        public static string GeteamNameUrl = "_apis/projects/5686e947-ee27-4250-b29c-1cafad947178/teams?api-version=1.0";

        public static string ReleaseUrl = "/ProjectOne/_apis/release/releases?api-version=3.0-preview";

        public static string TeamProjectUrl = "ProjectOne/_apis/wit/classificationnodes?$depth=2&api-version=2.0";

        public static string LINQUrl= "http://desktop-498rai4:8080/tfs/DefaultCollection/_apis/wit/wiql?api-version=2.0";
    }


    //Model class for Login 
    public class userLogin
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}