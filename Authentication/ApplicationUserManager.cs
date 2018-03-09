using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace ScreenViewer.Authentication
{
    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        public ApplicationUserManager()
          : base(new UserStore<ApplicationUser>(new ApplicationDbContext()))
        {
            this.PasswordHasher = new SqlPasswordHasher();
            this.MaxFailedAccessAttemptsBeforeLockout = 5;
            this.DefaultAccountLockoutTimeSpan = TimeSpan.FromDays(360);
        }
    }
}