using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace EnglishBot
{
    public static class ApiSetting
    {
        public static string Base = $"https://myengapideploy.azurewebsites.net/api/engapi/";

        public static HttpClient EngApiClient { get; set; }

        public static void InitializeClient()
        {
            EngApiClient = new HttpClient();
            EngApiClient.BaseAddress = new Uri("https://myengapideploy.azurewebsites.net/");
        }

    }
}
