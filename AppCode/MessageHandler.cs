using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Results;
using System.Collections;
using System.Reflection;
using System.Text;
using System.Net.Mail;
using System.Net.Http;
using System.Net.Http.Formatting;
using ScreenViewer.API;
using ScreenViewer.Models;
using ScreenViewer.Data;
using ScreenViewer.SessionControl;
using ScreenViewer.API.ExternalData;
using ScreenViewer.API.SendText;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Twilio;
using System.Threading.Tasks;
namespace ScreenViewer.AppCode
{


        public class MessageHandler : DelegatingHandler
        {

        public async Task<HttpResponseMessage> SendPost(HttpRequestMessage HRM, Uri suri)
        {
            using (var client = new HttpClient())
            {

                client.BaseAddress = suri;

                var returndata = client.SendAsync(HRM, HttpCompletionOption.ResponseContentRead).Result;
                return returndata;
            }


        }


    }


}