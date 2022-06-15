using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Text.Json;
using System.Web;
using System.Web.Mvc;
using System.Web.Services;
using FileSystem;
using WebApplication2.Classes;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    public partial class HomeController : Controller
    {
        public ActionResult GroupsFiles(string root, string catchall)
        {

            if (!AuthorizationCheck())
                return RedirectToAction("Login");

            string login, password;
            int id;

            GetCredentials(out id, out login, out password);

            ViewBagFill(id, login, password, catchall, context);

            ViewBag.OpenUsers = GetOpenUsersExceptOwner(login);
            var users_groups = GetUsersGroups(id);
            List<List<UsersInGroup>> list_list = new List<List<UsersInGroup>>();
            foreach (var grp in users_groups) 
            {
                list_list.Add(GetUsersViewByGroup(grp.group_name));
            }

            ViewBag.UsersGroups = users_groups;
            ViewBag.UsersInGroupView = list_list;

            ViewBag.root_parm = root;
            ViewBag.catchall_parm = catchall;

            ViewBag.isIndexPage = false;

            ViewBag.Action_page = "GroupsFiles";

            ViewBag.MyContext = context;

            if (root == null)
                return View();

            ViewBag.write_mode = false;
            var userGroup = from usrGrp in context.UserGroups
                            where usrGrp.user_group.Equals(root)
                            && usrGrp.user_id == id
                            select usrGrp;
            UserGroups check_write_mode = userGroup.ToList()[0];
            if (check_write_mode.C_writable == true)
                ViewBag.write_mode = true;

            var grp_search = context.Groups.Find(root);
            var grp_owner = context.Users.Find(grp_search.owner_id);
            ViewBag.group_owner = grp_owner!=null?grp_owner.fullname:user404;

            ViewBag.file_root = shared + root + "\\";
            ViewBag.groups_root = root;

            CurrentFolder folder = new CurrentFolder(login, shared + root + "\\" + catchall);

            ViewBag.folder = folder.GetAll();

            if (catchall == null || catchall.Length == 0)
            {
                ViewBag.IsRoot = true;
                ViewBag.CurrentPage = root;
            }
            else
            {
                ViewBag.IsRoot = false;
                string root_page = root;
                string root_url = hostname + $"Home/GroupsFiles/?root={root}&catchall=";
                ViewBag.BackPages = GetBackPages(catchall, root_page, root_url);
                ViewBag.CurrentPage = GetCurrentPage(catchall);
            }

            ViewBag.move_root_name = root + " /";

            return View("GroupsIndex");
        }

        public ActionResult ChangeProfile( string catchall, string password, string fullname, string allow_add, string action)
        {
            try
            {
                string login;
                GetCredentials(out login);

                catchall = catchall.Replace("\\", "/");

                UpdateProfile(login, password, fullname, allow_add);
            }
            catch
            {

            }
            return RedirectToAction(action, new { catchall = catchall });
        }

        public ActionResult CreateGroup()
        {
            string login, password;
            int id;

            GetCredentials(out id, out login, out password);

            CreateGroup_FileSystem();
            CreateGroup_DB(GetUser(login));

            return RedirectToAction("GroupsFiles");
        }

        public ActionResult UpdateGroup()
        {
            TryUpdateGroup(Request.Form["group_name"]);

            return RedirectToAction("GroupsFiles/");
        }

        public ActionResult DeleteGroup()
        {
            var grp_name = Request.Form["group_name"];

            DeleteGroup_DB(grp_name);
            DeleteGroup_FileSystem(grp_name);

            return RedirectToAction("GroupsFiles/");
        }

        public ActionResult UploadToGroups(HttpPostedFileBase upload, string path, string file_root, string root_parm, string catchall_parm)
        {
            string login, password;
            int id;

            GetCredentials(out id, out login, out password);
            
            UploadFile(login, upload, path, file_root);

            return RedirectToRoute(new { controller = "Home", action = "GroupsFiles", root = root_parm, catchall = catchall_parm });
        }

        public ActionResult MoveTo(string path, string name, string type, string action, string src_dir, string file_root,
            string root_parm, string catchall_parm)
        {
            string login;
            GetCredentials(out login);

            MovingFile(login, path, name, type, action, src_dir, file_root);

            if (file_root == null || file_root.Length == 0)
                return RedirectToAction(action, new { catchall = path });
            else
                return RedirectToRoute(new { controller = "Home", action = "GroupsFiles", root = root_parm, catchall = catchall_parm });
        }

        public ActionResult CopyTo(string path, string name, string type, string action, string src_dir, string file_root,
            string root_parm, string catchall_parm)
        {
            string login;
            GetCredentials(out login);

            CopyingFile(login, path, name, type, action, src_dir, file_root);

            if (file_root == null || file_root.Length == 0)
                return RedirectToAction(action, new { catchall = path });
            else
                return RedirectToRoute(new { controller = "Home", action = "GroupsFiles", root = root_parm, catchall = catchall_parm });
        }

        public ActionResult LeaveGroup(string group_name)
        {
            string login;
            GetCredentials(out login);

            int id = GetIdByLogin(login);

            var sel = (from usr_grp in context.UserGroups
                       where usr_grp.user_group.Equals(group_name)
                       && usr_grp.user_id == id
                       select usr_grp).ToList();

            UserGroups line = sel[0];

            context.UserGroups.Remove(line);
            context.SaveChanges();

            return RedirectToAction("GroupsFiles");
        }
    }
}