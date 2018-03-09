using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ScreenViewer.Authentication;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Host.SystemWeb;
using Microsoft.Owin.Security;
using ScreenViewer.Models;

namespace ScreenViewer.Controllers
{
    public class AccountController : Controller
    {
        
        public ActionResult Login(string returnUrl = "")
         {
            if (User.Identity.IsAuthenticated)
            {
                return LogOut();
            }

            if (Request.Cookies["myCookie"] != null)
            {
                var c = new HttpCookie("myCookie");
                c.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(c);
            }

            ScreenViewer.SessionControl.SessionManager.ClearSessionData(HttpContext.Session);

            ViewBag.ReturnUrl = returnUrl;

            if (string.IsNullOrEmpty(returnUrl))
            {
                ViewBag.ReturnUrl = Session["ReturnURL"];
            }

            return View();
        }

        [HttpPost]
        public ActionResult Login(Login model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                // Instantiate a new ApplicationUserManager and find a user based on provided Username
                var UserManager = new ApplicationUserManager();
                var user = UserManager.FindByName(model.UserName);
                ViewBag.ReturnUrl = returnUrl;

                if (user != null)
                {
                    if (UserManager.IsLockedOut(user.Id))
                    {
                        ModelState.AddModelError("", "Your account has been locked out because of too many invalid login attempts. Please contact your administrator.");
                    }
                    //else if (!user.IsApproved)
                    //{
                    //    ModelState.AddModelError("", "Your account has been deactivated. Please contact your administrator.");
                    //}
                    else if (!user.EmailConfirmed)
                    {
                        ModelState.AddModelError("", "Your email has not yet been confirmed. Please check you email.");
                    }
                    else
                    {
                        // Valid user, verify password
                        var result = UserManager.PasswordHasher.VerifyHashedPassword(user.PasswordHash, model.Password);
                        if (result == PasswordVerificationResult.Success)
                        {
                            if (UserManager.IsInRole(user.Id, "ADMINISTRATOR") || UserManager.IsInRole(user.Id, "AGENT") || UserManager.IsInRole(user.Id, "PROSPECT"))
                            {
                                UserAuthenticated(UserManager, user);
                                return RedirectToLocal(returnUrl);
                            }
                            else
                            {
                                ModelState.AddModelError("", "You do not have the privilege required to login here. Please contact your administrator.");
                            }
                        }
                        else if (result == PasswordVerificationResult.SuccessRehashNeeded)
                        {
                            // Logged in using old Membership credentials - update hashed password in database
                            // Since we update the user on login anyway, we'll just set the new hash
                            // Optionally could set password via the ApplicationUserManager by using
                            // RemovePassword() and AddPassword()
                            user.PasswordHash = UserManager.PasswordHasher.HashPassword(model.Password);
                            UserAuthenticated(UserManager, user);
                            return RedirectToLocal(returnUrl);
                        }
                        else
                        {
                            // Failed login, increment failed login counter
                            // Lockout for 15 minutes if more than 10 failed attempts
                            user.AccessFailedCount++;
                            if (user.AccessFailedCount >= 10)
                            {
                                user.LockoutEndDateUtc = DateTime.UtcNow.AddMinutes(15);
                                ModelState.AddModelError("", "Please wait 15 min and try again.");
                            }
                            else
                                ModelState.AddModelError("", "Login failed. Please check your user name and password and try again.");

                            UserManager.Update(user);
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Login failed. Please check your user name and password and try again.");
                }
            }

            return View(model);
        }

        private void UserAuthenticated(ApplicationUserManager userManager, ScreenViewer.Authentication.ApplicationUser user)
        {
            user.AccessFailedCount = 0;
            user.LockoutEndDateUtc = null;

            //create a cookie
            HttpCookie myCookie = new HttpCookie("myCookie");

            //Add key-values in the cookie
            myCookie.Values.Add("UserId", user.Id);
            myCookie.Values.Add("ClientId", ScreenViewer.ClientHelper.GetClientIdByUserID(user.Id));
            myCookie.Values.Add("AgentName", ScreenViewer.ClientHelper.GetUserNameByID(user.Id));

            //set cookie expiry date-time. Made it to last for next 7 hours.
            myCookie.Expires = DateTime.Now.AddHours(8);
            Response.Cookies.Add(myCookie);

            userManager.Update(user);
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        public ActionResult LogOut()
        {
            if (Request.Cookies["myCookie"] != null)
            {
                var c = new HttpCookie("myCookie");
                c.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(c);
            }

            return RedirectToAction("Login", "Account", null);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            if (Request.Cookies["myCookie"] != null)
            {
                var c = new HttpCookie("myCookie");
                c.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(c);
            }

            return RedirectToAction("Index", "Home");
        }
    }
}