using System;
using System.Web;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Identity;
using ScreenViewer.Authentication;

namespace ScreenViewer
{
    public static class SignInHelper
    {
        public static bool ValidateSignIn()
        {
            try
            {
                bool isValid = false;

                string environment = ConfigurationManager.AppSettings["environment"];

                if (environment.ToUpper() == "PRODUCTION")
                {
                    HttpCookie myCookie = HttpContext.Current.Request.Cookies["myCookie"];

                    if (myCookie != null && !string.IsNullOrEmpty(myCookie.Values["UserId"]) && !string.IsNullOrEmpty(myCookie.Values["ClientId"]))
                        isValid = true;

                }
                else
                {
                    string userId = HttpContext.Current.Request.QueryString["UserId"];
                    string clientId = HttpContext.Current.Request.QueryString["ClientId"];

                    if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(clientId))
                    {
                        if (ClientHelper.ValidateUserIdAndClientId(userId, clientId))
                        {
                            var UserManager = new ApplicationUserManager();
                            var user = UserManager.FindById(userId);

                            if (user != null)
                            {
                                HttpCookie myCookie = new HttpCookie("myCookie");

                                //Add key-values in the cookie
                                myCookie.Values.Add("UserId", user.Id);
                                myCookie.Values.Add("ClientId", ScreenViewer.ClientHelper.GetClientIdByUserID(user.Id));
                                myCookie.Values.Add("AgentName", ScreenViewer.ClientHelper.GetUserNameByID(user.Id));

                                //myCookie.Values.Add("UserId", userId);
                                //myCookie.Values.Add("ClientId", clientId);
                                //myCookie.Expires = DateTime.Now.AddHours(8);

                                //set cookie expiry date-time. Made it to last for next 7 hours.
                                myCookie.Expires = DateTime.Now.AddHours(8);
                                HttpContext.Current.Response.Cookies.Add(myCookie);

                                isValid = true;
                            }
                        }
                    }
                    else
                    {
                        HttpCookie myCookie = HttpContext.Current.Request.Cookies["myCookie"];

                        if (myCookie != null && !string.IsNullOrEmpty(myCookie.Values["UserId"]) && !string.IsNullOrEmpty(myCookie.Values["ClientId"]))
                            isValid = true;
                    }
                }

                return isValid;
            }
            catch
            {
                return false;
            }
        }
    }
}