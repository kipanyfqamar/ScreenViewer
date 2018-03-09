using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Twilio; 
namespace ScreenViewer.API.SendText
{
    public class TwillioSMSController : ApiController
    {

        public void Send(string from, string to, string message)
        {
            // Find your Account Sid and Auth Token at twilio.com/user/account 
            string AccountSid = "AC8f7a6be32ca6a082664fb113b8ebd09c";
      //      AccountSid = "ACd42bd3295ac26901ef5f1e6a1f46916a"; //test account
            string AuthToken = "1e7790d05b2380140b7db266f2392dfd";
       //     AuthToken = "78af6f60c6d0e82347ef13308c0cb57b"; //test account
            var twilio = new TwilioRestClient(AccountSid, AuthToken);
                string url;
            var themessage = twilio.SendMessage(from, to, message);
            //var themessage = twilio.SendMessage("+15005550006", to, message);

            
        }
    }

}

