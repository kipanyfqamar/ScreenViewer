using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity.EntityFramework;

namespace ScreenViewer.Authentication
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        // throwIfV1Schema throws an exception if an existing v.1 Schema
        // exists for ASP.NET Identity
        public ApplicationDbContext()
          :  base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

    }
}