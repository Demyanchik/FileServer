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
        static string admin_group = "Admin";

        void ClearTemp(string login) 
        {
            Directory.Delete(temp + login, true);
            Directory.CreateDirectory(temp + login);
        }

        void CreateUser_DB() 
        {
            Users user = new Users();
            user.login = Request.Form["login"];
            user.password = Request.Form["password"];
            user.fullname = Request.Form["fullname"];
            user.role = "User";

            user.quota_max = quota_default;
            user.quota_current = 0;

            context.Users.Add(user);
            context.SaveChanges();

            for (int i = 1; i <= context.Groups.Count(); i++)
            {
                string group = null;
                if ((group = Request.Form["group" + i]) != null)
                {
                    if (group.Equals("Admin"))
                        user.role = "Admin";
                    context.UserGroups.Add(new UserGroups { user_id = user.id, user_group = group });
                }
            }
            context.SaveChanges();
        }
        void CreateUser_FileSystem() 
        {
            string login = Request.Form["login"];
            if (!Directory.Exists(users_base + login)) 
            {
                Directory.CreateDirectory(users_base + login);
                Directory.CreateDirectory(temp + login);

                Directory.CreateDirectory(GetUserPath(login));
                Directory.CreateDirectory(GetDownloadPath(login));
                Directory.CreateDirectory(GetGarbagePath(login));

                //Directory.CreateDirectory(shared + login);
                //Directory.CreateDirectory(shared + login + @"\links");
                //Directory.CreateDirectory(shared + login + @"\users");
                //Directory.CreateDirectory(shared + login + @"\groups");


                DirectoryCopy(@"E:\MyCloud\new_user_template", GetUserPath(login), true);
            }
        }

        void DeleteUser_FileSystem(string login) 
        {
            Directory.Delete(users_base + login, true);
            Directory.Delete(temp + login, true);
        }
        void DeleteUser_DB(int id)
        {
            var user = context.Users.Find(id);

            context.Users.Remove(user);

            context.SaveChanges();
        }

        void UpdateUser(int i) 
        {
            int id = Int32.Parse(Request.Form["id" + i]);

            var user = context.Users.Find(id);

            //context.Users.Remove(user);
            //context.SaveChanges();

            user.login = Request.Form[$"login{i}_updatable"];
            user.password = Request.Form["password" + i];
            user.fullname = Request.Form["fullname" + i];
            user.role = "User";
            user.quota_max = SetQuota(Request.Form["quota" + i]);
            user.UserGroups.Clear();

            //var user_groups = (from _group in context.UserGroups
            //                               where _group.user_name.Equals(login)
            //                               select _group).ToList();

            ////user.UserGroups.Clear();

            //foreach (var group in user_groups) 
            //{
            //    context.UserGroups.Remove(group);
            //}


            for (int j = 1; j <= context.Groups.Count(); j++)
            {
                string group = null;
                if ((group = Request.Form[$"users{i}_group{j}"]) != null)
                {
                    if (group.Equals("Admin"))
                        user.role = "Admin";

                    bool write_mode = bool.Parse(Request.Form[$"users{i}_group{j}_write_mode"]);
                    user.UserGroups.Add(new UserGroups { user_id = user.id, user_group = group, C_writable = write_mode });
                }
            }

            context.SaveChanges();
        }

        int SetQuota(string option)
        {
            switch (option) 
            {
                case "По умолчанию":
                    return 512;
                case "1Gb":
                    return 1024;
                case "2Gb":
                    return 2 * 1024;
                case "5Gb":
                    return 5 * 1024;
                case "10Gb":
                    return 10 * 1024;
            }

            return 0;
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the destination directory doesn't exist, create it.       
            Directory.CreateDirectory(destDirName);

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string tempPath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
                }
            }
        }

        public static bool IsSharedLink(string user, string path)
        {
            int id = GetIdByLogin(user);
            var users_shared = (from shared in context.Shared
                                where shared.user_id == id
                                && shared.target_type.Equals("link")
                                select shared).ToList();

            foreach (var shared in users_shared)
            {
                if (shared.path.Equals(path))
                    return true;
            }

            return false;
        }

        public static string GetSharedLink(string user, string path)
        {
            int id = GetIdByLogin(user);
            var users_shared = (from shared in context.Shared
                                where shared.user_id == id
                                && shared.target_type.Equals("link")
                                select shared).ToList();

            foreach (var shared in users_shared)
            {
                if (shared.path.Equals(path))
                    return shared.link_url;
            }

            return null;
        }

        public bool CheckAdminRights() 
        {
            string user = null;
            GetCredentials(out user);
            int user_id = GetIdByLogin(user);

            var user_groups = (from groups in context.UserGroups
                              where groups.user_id == user_id
                              select groups).ToList();

            foreach (var group in user_groups)
            {
                if (group.user_group.Equals(admin_group))
                    return true;
            }

            return false;
        }
    }
}