using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    public partial class HomeController : Controller
    {
        void ViewBagFill(int id, string login, string password, string catchall, PvI_KP_Entities context) 
        {
            ProcessStorageSpace(id, login, context);

            if (catchall != null)
                catchall = catchall.Replace("/", @"\");

            ViewBag.context = context;
            ViewBag.login = login;

            Users user = GetUser(login);
            ViewBag.userInfo = user;

            string progress = (((double)user.quota_current / (double)user.quota_max) * 100).ToString().Replace(",", ".");
            ViewBag.progress = progress;

            int? quota_free = user.quota_max - user.quota_current;
            if (quota_free < 0)
                quota_free = 0;

            string free = "";
            int pow = 1024;
            if (quota_free >= pow)
                free = Math.Round((float)quota_free / pow, 2) + "Gb";
            else
                free = quota_free + "Mb";

            ViewBag.free = free;

            string max = "";
            if (user.quota_max >= pow)
                max = Math.Round((float)user.quota_max / pow, 2) + "Gb";
            else
                max = user.quota_max + "Mb";

            ViewBag.max = max;

            var sel = (from usr in context.UserGroups
                      where usr.user_id == id
                      && usr.user_group.Equals("Admin")
                      select usr).ToList();
            bool is_admin = user.role.Equals("Admin") || (sel.Count > 0? true: false);
            ViewBag.show_admin_panel = is_admin;

            ViewBag.path = catchall;
        }
    }
}