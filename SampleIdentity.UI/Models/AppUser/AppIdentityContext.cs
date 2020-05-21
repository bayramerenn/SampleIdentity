

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleIdentity.UI.Models.AppUser
{
    public class AppIdentityContext : IdentityDbContext<AppUser,AppRole,string>
    {
        public AppIdentityContext(DbContextOptions<AppIdentityContext> options):base(options)
        {

        }
    }
}
