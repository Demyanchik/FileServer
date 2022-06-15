using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services;

namespace WebApplication2.Controllers
{
    public partial class HomeController : Controller
    {
        [WebMethod]
        public string GenerateLink(string user, string path)
        {
            int? new_user_link = UsersNewLinkNumber(user);

            string query = RSA.Encryption($"user={user}&link={new_user_link}");
            return hostname + "Home/Link?code=" + query;
        }

        [WebMethod]
        public int CheckUsers(string user)
        {
            var users = (from usr in context.Users
                         where usr.login.Equals(user)
                         select usr).ToList();

            if (users.Count > 0)
                return -1;
            else
                return 1;
        }

        [WebMethod]
        public int CheckGroups(string group_name)
        {
            var groups = (from grp in context.Groups
                         where grp.group_name.Equals(group_name)
                         select grp).ToList();

            if (groups.Count > 0)
                return -1;
            else
                return 1;
        }
    }
}