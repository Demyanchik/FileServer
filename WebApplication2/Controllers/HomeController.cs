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
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    public partial class HomeController : Controller
    {
        public static string hostname2 = "http://localhost:60447/"; 
        public static string hostname = "http://localhost:44318/";

        static string users_base = @"E:\MyCloud\Users\";
        string temp = @"E:\MyCloud\Temp\";
        string shared = @"E:\MyCloud\Shared\";

        public static string user404 = "Пользователь не найден";

        int quota_default = 512;

        BigInteger bytes_to_megas = 1024 * 1024;

        static PvI_KP_Entities context = new PvI_KP_Entities();

        public ActionResult Redirect(string catchall) 
        {
            return RedirectToAction("Index");
        }
        public ActionResult Index(string catchall)
        {
            if (!AuthorizationCheck())
                return RedirectToAction("Login");

            string login, password;
            int id;

            GetCredentials(out id, out login, out password);

            ViewBagFill(id, login, password, catchall, context);

            CurrentFolder folder = new CurrentFolder(login, GetUserPath(login) + catchall);

            ViewBag.folder = folder.GetAll();

            if (catchall == null)
            {
                ViewBag.IsRoot = true;
                ViewBag.CurrentPage = "Файлы";
            }
            else 
            {
                ViewBag.IsRoot = false;
                ViewBag.BackPages = GetBackPages(catchall, 1);
                ViewBag.CurrentPage = GetCurrentPage(catchall);
            }

            ViewBag.Action_page = "Index";

            ViewBag.file_root = null;
            ViewBag.move_root_name = "Файлы /";

            return View();
        }
        public ActionResult Users(string catchall)
        {
            if (!AuthorizationCheck())
                return RedirectToAction("Login");

            if (!CheckAdminRights()) 
            {
                return new HttpStatusCodeResult(403);
            }

            string login, password;
            int id;

            GetCredentials(out id, out login, out password);

            ViewBagFill(id, login, password, catchall, context);

            ViewBag.users = context.Users.ToList();
            ViewBag.all_groups = context.Groups.ToList();

            ViewBag.Action_page = "Users";

            return View();
        }
        public ActionResult LogOut()
        {
            SetCookie("login", null);
            SetCookie("password", null);

            SetSession("login", null);
            SetSession("password", null);

            return RedirectToAction("Login");
        }
        public ActionResult History(string catchall) 
        {
            if (!AuthorizationCheck())
                return RedirectToAction("Login");

            string login, password;
            int id;

            GetCredentials(out id, out login, out password);

            ViewBagFill(id, login, password, catchall, context);

            return View();
        }
        public ActionResult Garbage(string catchall)
        {
            if (!AuthorizationCheck())
                return RedirectToAction("Login");

            string login, password;
            int id;

            GetCredentials(out id, out login, out password);

            ViewBagFill(id, login, password, catchall, context);

            CurrentFolder folder = new CurrentFolder(login, GetGarbagePath(login) + catchall);

            ViewBag.folder = folder.GetAll();

            if (catchall == null)
            {
                ViewBag.IsRoot = true;
                ViewBag.CurrentPage = "Корзина";
            }
            else
            {
                ViewBag.IsRoot = false;
                ViewBag.BackPages = GetBackPages(catchall, 3);
                ViewBag.CurrentPage = GetCurrentPage(catchall);
            }

            ViewBag.Action_page = "Garbage";

            return View();
        }
        public ActionResult Login()
        {
            if (Request.Form.Count == 0)
                return View();

            var login = Request.Form["login"];
            var password = Request.Form["password"];
            int status;

            if ((status = TryLogin(login, password)) > 0) 
            {
                //...
                if (Request.Form["stay"] != null)
                {
                    SetCookie("login", login);
                    SetCookie("password", password);
                }
                else
                {
                    SetSession("login", login);
                    SetSession("password", password);
                }

                return RedirectToAction("Index");
            }

            ViewBag.login = login;
            ViewBag.password = password;
            ViewBag.status = status;

            return View();
        }
        public ActionResult UploadFile(HttpPostedFileBase upload, string path) 
        {
            string login, password;
            int id;

            GetCredentials(out id, out login, out password);


            if (!CheckSpace(id, upload))
            {
                TempData["toast_message"] = $"Файл \"{upload.FileName}\" не был загружен, поскольку недостаточно места.";
                TempData["toast"] = true;
            }
            else 
            {
                UploadFile(login, upload, path, null);
            }
            return RedirectToAction("Index", new { catchall = path });
        }
        public FileResult DownloadFile(string path, string name, string type, string action, string file_root) 
        {
            string login;
            GetCredentials(out login);

            return Download(login, path, name, type, action, file_root);
        }
        public ActionResult SaveToDownloads(string code, string type)
        {
            string login = null;
            GetCredentials(out login);

            var query_string = RSA.Decryption(code);
            NameValueCollection query = HttpUtility.ParseQueryString(query_string);

            string user = query["user"];
            int user_id = GetIdByLogin(user);

            int link = int.Parse(query["link"]);

            var item = (from shared in context.Shared
                        where shared.target_type.Equals("link")
                        && shared.user_id == user_id
                        && shared.link_number == link
                        select shared).ToList();

            string path = GetUserPath(user) + item[0].path;
            string fileName = item[0].path.Substring(item[0].path.LastIndexOf('/') + 1);

            if (type.Equals("File"))
            {
                FileInfo past;
                if ((past = new FileInfo(GetDownloadPath(login) + fileName)).Exists)
                    past.Delete();

                System.IO.File.Copy(path, GetDownloadPath(login) + fileName);
            }
            else 
            {
                DirectoryInfo past;
                if ((past = new DirectoryInfo(GetDownloadPath(login) + fileName)).Exists)
                    past.Delete(true);

                CopyDirectory(new DirectoryInfo(path), new DirectoryInfo(GetDownloadPath(login) + fileName));
            }

            return RedirectToAction("Downloads");
        }
        public FileResult DownloadFromLink(string code, string type)
        {
            var query_string = RSA.Decryption(code);
            NameValueCollection query = HttpUtility.ParseQueryString(query_string);

            string user = query["user"];
            int user_id = GetIdByLogin(user);

            int link = int.Parse(query["link"]);

            var item = (from shared in context.Shared
                        where shared.target_type.Equals("link")
                        && shared.user_id == user_id
                        && shared.link_number == link
                        select shared).ToList();



            return Download(user, item[0].path, item[0].type);
        }
        public ActionResult RemoveFile(string path, string name, string type, string action, string file_root, string groups_root)
        {
            string login;
            GetCredentials(out login);

            RemoveToBasket(login, path, name, type, action, file_root);

            if (action.Equals("Index"))
                action = "Root";

            if (groups_root != null && groups_root.Length > 0)
                action = "Shared";

            RemovedFiles line = new RemovedFiles()
            {
                user_id = GetIdByLogin(login),
                path = action + @"\" + path + name,
                type = type
            };
            context.RemovedFiles.Add(line);
            context.SaveChanges();

            if (action.Equals("Root"))
                action = "Index";
            if (action.Equals("Shared"))
                action = "GroupsFiles";

            if (file_root == null || file_root.Length == 0)
                return RedirectToAction(action, new { catchall = path });
            else
                return RedirectToAction(action);
        }
        public ActionResult RemovePermanent(string path, string name, string type, string action, string file_root, string root_parm, string catchall_parm)
        {
            string login;
            GetCredentials(out login);

            Remove(login, path, name, type, action, file_root);

            if(root_parm == null)
                return RedirectToAction(action, new { catchall = path });
            else
                return RedirectToRoute(new { controller = "Home", action = "GroupsFiles", root = root_parm, catchall = catchall_parm });
        }
        public ActionResult Restore(string path, string name, string type, string action)
        {
            try
            {
                string login;
                GetCredentials(out login);
                int id = GetIdByLogin(login);

                var users_removed = (from removed in context.RemovedFiles
                                     where removed.user_id == id
                                     && removed.path.Contains(path + name)
                                     && removed.type.Equals(type)
                                     select removed).ToList();

                string dest;
                if (users_removed.Count == 0)
                    dest = @"Root\" + name;
                else
                    dest = users_removed[0].path;

                string root = null;
                switch (action)
                {
                    case "Index":
                        root = GetUserPath(login);
                        break;
                    case "Downloads":
                        root = GetDownloadPath(login);
                        break;
                    case "Garbage":
                        root = GetGarbagePath(login);
                        break;
                }

                string file_path = root + path + name;

                if (type.Equals("File"))
                {
                    FileInfo fileInf = new FileInfo(file_path);
                    if (fileInf.Exists)
                    {
                        FileInfo past_file;
                        if ((past_file = new FileInfo(users_base + login + "\\" + dest)).Exists)
                            past_file.Delete();

                        System.IO.File.Move(file_path, users_base + login + "\\" + dest);
                    }
                    else
                        throw new Exception("Файл не найден.");
                }
                else
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(file_path);
                    if (dirInfo.Exists)
                    {
                        DirectoryInfo past_dir;
                        if ((past_dir = new DirectoryInfo(users_base + login + "\\" + dest)).Exists)
                            past_dir.Delete(true);
                        Directory.Move(file_path, users_base + login + "\\" + dest);
                    }
                    else
                        throw new Exception("Каталог не найден.");
                }

                if (users_removed.Count > 0)
                {
                    context.RemovedFiles.Remove(users_removed[0]);
                    context.SaveChanges();
                }
            }
            catch 
            {

            }
            
            
            return RedirectToAction(action, new { catchall = path });
        }
        public ActionResult CreateUser()
        {
            //context.UserGroups.Add(new UserGroups{ user_name = "user1", user_group = "ИСиТ 1" });;
            //context.SaveChanges();
            CreateUser_FileSystem();
            CreateUser_DB();

            return RedirectToAction("Users");
        }
        public ActionResult DeleteUser() 
        {
            string login = Request.Form["login"];
            string remove_files = Request.Form["remove_files"];
            int id = GetIdByLogin(login);

            DeleteUser_DB(id);

            if (remove_files != null)
                DeleteUser_FileSystem(login);

            return RedirectToAction("Users");
        }
        public ActionResult UpdateUsers()
        {
            for (int i = 1; i <= context.Users.Count(); i++) 
            {
                UpdateUser(i);
            }

            return RedirectToAction("Users");
        }
        public ActionResult SharedWithYou() 
        {
            if (!AuthorizationCheck())
                return RedirectToAction("Login");

            string login, password;
            int id;

            GetCredentials(out id, out login, out password);

            ViewBagFill(id, login, password, null, context);

            return View();
        }
        int? UsersNewLinkNumber(string user) 
        {
            int user_id = GetIdByLogin(user);

            var user_links = (from links in context.Shared
                              where links.target_type.Equals("link")
                              && links.user_id == user_id
                              orderby links.link_number descending
                              select links).ToList();

            int? new_user_link;
            if (user_links.Count > 0)
                new_user_link = user_links[0].link_number + 1;
            else
                new_user_link = 1;

            return new_user_link;
        }
        public ActionResult CreateLink(string link_url, string user, string path, string type, string catchall) 
        {
            int? users_id = GetIdByLogin(user);
            var link = new Shared
            {
                user_id = users_id,
                link_number = UsersNewLinkNumber(user),
                path = path,
                type = type,
                target_type = "link",
                link_url = link_url
            };

            context.Shared.Add(link);
            context.SaveChanges();

            return RedirectToAction("Index", new { catchall = catchall });
        }
        public ActionResult CloseLink(string link_url, string catchall)
        {
            var link = (from links in context.Shared
                       where links.link_url.Equals(link_url)
                       select links).ToList()[0];

            context.Shared.Remove(link);
            context.SaveChanges();

            return RedirectToAction("Index", new { catchall = catchall });
        }
        public ActionResult Link(string code)
        {

            bool authorized = AuthorizationCheck();
            ViewBag.Authorized = authorized;
            if (authorized)
            {
                string login = null;
                GetCredentials(out login);
                ViewBag.login = login;
            }

            if (code == null)
            {
                ViewBag.Found = false;
                return View();
            }

            var query_string = RSA.Decryption(code);
            NameValueCollection query = HttpUtility.ParseQueryString(query_string);

            string user = query["user"];
            int user_id = GetIdByLogin(user);

            int link = int.Parse(query["link"]);

            var item = (from shared in context.Shared
                        where shared.target_type.Equals("link")
                        && shared.user_id == user_id
                        && shared.link_number == link
                        select shared).ToList();

            if (item.Count == 0)
            {
                ViewBag.Found = false;
                return View();
            }
            ViewBag.Found = true;

            if (item[0].type.Equals("File"))
            {
                ViewBag.Type = "File";

                Item file = new Item();
                string file_path = GetUserPath(user) + item[0].path;
                var file_info = new FileInfo(file_path);
                file.Icon = CurrentFolder.GetFileIcon(file_path);
                file.Name = file_info.Name;
                file.Type = "File";

                ViewBag.Item = file;

                Users user_info = (from users in context.Users
                                   where users.login.Equals(user)
                                   select users).ToList()[0];
                ViewBag.Owner = user_info.fullname;
                ViewBag.Size = GetFileSize(file_info.Length);
                ViewBag.Changed = file_info.LastWriteTime;

            }
            else
            {
                ViewBag.Type = "Folder";

                Item dir = new Item();
                string dir_path = GetUserPath(user) + item[0].path;
                var dir_info = new DirectoryInfo(dir_path);
                dir.Icon = CurrentFolder.GetFolderIcon();
                dir.Name = dir_info.Name;
                dir.Type = "Folder";

                ViewBag.Item = dir;

                Users user_info = (from users in context.Users
                                   where users.login.Equals(user)
                                   select users).ToList()[0];
                ViewBag.Owner = user_info.fullname;
                ViewBag.Size = GetFileSize(GetDirectorySize(dir_info));
                ViewBag.Changed = dir_info.LastWriteTime;
            }

            return View();
        }
        public ActionResult NewFolder(string folder, string catchall, string file_root, string root_parm, string catchall_parm) 
        {
            string user;

            GetCredentials(out user);
            if (file_root == null)
            {
                Directory.CreateDirectory(GetUserPath(user) + catchall + folder);

                return RedirectToAction("Index", new { catchall = catchall });
            }
            else 
            {
                Directory.CreateDirectory(file_root + catchall + folder);

                return RedirectToRoute(new { controller = "Home", action = "GroupsFiles", root = root_parm, catchall = catchall_parm });
            }
        }

        public ActionResult NewGroupFolder(string folder, string catchall)
        {
            string user;

            GetCredentials(out user);

            Directory.CreateDirectory(GetUserPath(user) + catchall + folder);

            return RedirectToAction("Index", new { catchall = catchall });
        }

        public ActionResult Downloads(string catchall)
        {
            if (!AuthorizationCheck())
                return RedirectToAction("Login");

            string login, password;
            int id;

            GetCredentials(out id, out login, out password);

            ViewBagFill(id, login, password, catchall, context);

            CurrentFolder folder = new CurrentFolder(login, GetDownloadPath(login) + catchall);

            ViewBag.folder = folder.GetAll();

            if (catchall == null)
            {
                ViewBag.IsRoot = true;
                ViewBag.CurrentPage = "Загрузки";
            }
            else
            {
                ViewBag.IsRoot = false;
                ViewBag.BackPages = GetBackPages(catchall, 2);
                ViewBag.CurrentPage = GetCurrentPage(catchall);
            }

            ViewBag.Action_page = "Downloads";
            ViewBag.move_root_name = "Загрузки /";

            return View();
        }
        public ActionResult Rename(string new_name, string path, string name, string type, string action, string file_root, string root_parm, string catchall_parm) 
        {
            try 
            {
                string login;
                GetCredentials(out login);


                RenameItem(new_name, login, path, name, type, action, file_root);
            }
            catch 
            {

            }
            if (root_parm == null || root_parm.Length == 0)
                return RedirectToAction(action, new { catchall = path });
            else
            {
                return RedirectToRoute(new { controller = "Home", action = "GroupsFiles", root = root_parm, catchall = catchall_parm });
            }
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}