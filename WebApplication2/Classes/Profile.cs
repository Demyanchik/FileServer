using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    public partial class HomeController : Controller
    {
        void UpdateProfile(string login, string password, string fullname, string allow_add)
        {
            var id = GetIdByLogin(login);
            var user = context.Users.Find(id);

            user.password = password;
            user.fullname = fullname;

            bool allow_mode = false;
            if (allow_add != null)
                allow_mode = true;

            user.allow_add = allow_mode;

            context.SaveChanges();
        }
    }
}