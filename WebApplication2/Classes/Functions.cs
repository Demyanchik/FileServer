using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using WebApplication2.Models;

using System.IO.Compression;
using System.IO;
using System.Numerics;

namespace WebApplication2.Controllers
{
    public partial class HomeController : Controller
    {
        Users GetUserInfo(string login, string password)
        {
            var get = from usr in context.Users
                      where usr.login.Equals(login)
                      select usr;

            var user = get.ToList();
            if (user.Count == 0 || (!user[0].password.Equals(password)))
                return null;

            return user[0];
        }

        Users GetUser(string login)
        {
            var get = from usr in context.Users
                      where usr.login.Equals(login)
                      select usr;

            var user = get.ToList();
            if (user.Count == 0)
                return null;

            return user[0];
        }

        int TryLogin(string login, string password)
        {
            var get = from usr in context.Users
                      where usr.login.Equals(login)
                      select usr;

            var user = get.ToList();
            if (user.Count == 0)
                return -1;

            if (!user[0].password.Equals(password))
                return -2;

            return 1;
        }

        static string GetUserPath(string login)
        {
            return users_base + login + @"\Root\";
        }
        static string GetDownloadPath(string login)
        {
            return users_base + login + @"\Downloads\";
        }
        static string GetGarbagePath(string login)
        {
            return users_base + login + @"\Garbage\";
        }

        string GetGroupPath(string group_name)
        {
            return shared + group_name + @"\";
        }

        bool UploadFile(string login, HttpPostedFileBase upload, string path, string file_root)
        {
            if (upload != null)
            {
                // получаем имя файла
                string fileName = System.IO.Path.GetFileName(upload.FileName);
                // сохраняем файл в папку Files в проекте
                string root;
                if (file_root == null)
                    root = GetUserPath(login);
                else
                    root = file_root;
                upload.SaveAs(root + path + fileName);
                return true;
            }

            return false;
        }

        bool CheckSpace(int id, HttpPostedFileBase upload) 
        {
            var user = context.Users.Find(id);
            BigInteger? free_space_bytes = (user.quota_max - user.quota_current) * bytes_to_megas;

            if(upload.ContentLength > free_space_bytes)
                return false;

            return true;
        }

        public FileResult Download(string login, string path, string fileName, string type, string action, string file_root)
        {
            string root = null;
            if (file_root == null || file_root.Length == 0)
            {
                switch (action)
                {
                    case "Index":
                        root = GetUserPath(login);
                        break;
                    case "Downloads":
                        root = GetDownloadPath(login);
                        break;
                }
            }
            else 
            {
                root = file_root;
            }

            string file_path = root + path + fileName;

            if (type.Equals("File"))
            {
                string file_type = fileName.Substring(fileName.LastIndexOf('.') + 1, fileName.Length - 1 - fileName.LastIndexOf('.'));
                string file_name = fileName;
                return File(file_path, file_type, file_name);
            }
            else 
            {
                //...
                string archive_name = fileName + ".zip";
                string zipFile = temp + login + @"\" + archive_name;

                FileInfo rewrite = new FileInfo(zipFile);
                if (rewrite.Exists)
                    rewrite.Delete();

                ZipFile.CreateFromDirectory(file_path, zipFile);

                return File(zipFile, "zip", archive_name);
            }
        }

        public FileResult Download(string login, string full_path, string type)
        {
            string file_path = GetUserPath(login) + full_path;

            string fileName = full_path.Substring(full_path.LastIndexOf('/') + 1);

            if (type.Equals("File"))
            {
                string file_type = fileName.Substring(fileName.LastIndexOf('.') + 1, fileName.Length - 1 - fileName.LastIndexOf('.'));
                string file_name = fileName;
                return File(file_path, file_type, file_name);
            }
            else
            {
                //...
                string archive_name = fileName + ".zip";
                string zipFile = temp + login + @"\" + archive_name;

                FileInfo rewrite = new FileInfo(zipFile);
                if (rewrite.Exists)
                    rewrite.Delete();

                ZipFile.CreateFromDirectory(file_path, zipFile);

                return File(zipFile, "zip", archive_name);
            }
        }

        void Remove(string login, string path, string fileName, string type, string action, string file_root) 
        {
            string root = null;
            if (file_root == null || file_root.Length == 0)
            {
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

                var shared = (from shr in context.Shared
                              where shr.path.Equals(path + fileName)
                              select shr).ToList();

                if (shared.Count > 0)
                {
                    context.Shared.Remove(shared[0]);
                    context.SaveChanges();
                }
            }
            else 
            {
                root = file_root;
            }

            string file_path = root + path + fileName;

            if (type.Equals("File"))
            {
                FileInfo fileInf = new FileInfo(file_path);
                if (fileInf.Exists)
                {
                    fileInf.Delete();
                    return;
                }
                throw new Exception("Файл не найден.");
            }
            else
            {
                DirectoryInfo dirInfo = new DirectoryInfo(file_path);
                if (dirInfo.Exists) 
                {
                    dirInfo.Delete(true);
                    return;
                }
                throw new Exception("Каталог не найден.");
            }
        }

        void RenameItem(string new_name, string login, string path, string fileName, string type, string action, string file_root)
        {
            string root = null;
            if (file_root == null || file_root.Length == 0)
            {
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
            }
            else 
            {
                root = file_root;
            }

            string file_path = root + path + fileName;
            string new_file = root + path + new_name;

            if (file_root == null) 
            {
                var shared = (from shr in context.Shared
                              where shr.path.Equals((path + fileName).Replace("\\", "/"))
                              && shr.target_type.Equals("link")
                              select shr).ToList();

                if (shared.Count > 0)
                {
                    shared[0].path = (path + new_name).Replace("\\", "/");
                    context.SaveChanges();
                }
            }

            if (type.Equals("File"))
            {
                FileInfo fileInf = new FileInfo(file_path);
                if (fileInf.Exists)
                {
                    System.IO.File.Move(file_path, new_file);
                    return;
                }
                throw new Exception("Файл не найден.");
            }
            else
            {
                DirectoryInfo dirInfo = new DirectoryInfo(file_path);
                if (dirInfo.Exists)
                {
                    Directory.Move(file_path, new_file);
                    return;
                }
                throw new Exception("Каталог не найден.");
            }
        }
        void RemoveToBasket(string login, string path, string fileName, string type, string action, string file_root)
        {
            string root = null;
            if (file_root == null || file_root.Length == 0)
            {
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
            }
            else
                root = file_root;

            string file_path = root + path + fileName;

            if (type.Equals("File"))
            {
                FileInfo fileInf = new FileInfo(file_path);
                if (fileInf.Exists)
                {
                    FileInfo past_file;
                    if ((past_file = new FileInfo(GetGarbagePath(login) + fileName)).Exists)
                        past_file.Delete();

                    System.IO.File.Move(file_path, GetGarbagePath(login) + fileName);
                    return;
                }
                throw new Exception("Файл не найден.");
            }
            else
            {
                DirectoryInfo dirInfo = new DirectoryInfo(file_path);
                if (dirInfo.Exists)
                {
                    DirectoryInfo past_dir;
                    if ((past_dir = new DirectoryInfo(GetGarbagePath(login) + fileName)).Exists)
                        past_dir.Delete(true);
                    Directory.Move(file_path, GetGarbagePath(login) + fileName);
                    return;
                }
                throw new Exception("Каталог не найден.");
            }
        }

        void MovingFile(string login, string path, string fileName, string type, string action, string src_dir, string file_root)
        {
            string root = null;
            if (file_root == null || file_root.Length == 0)
            {
                switch (action)
                {
                    case "Index":
                        root = GetUserPath(login);
                        break;
                    case "Downloads":
                        root = GetDownloadPath(login);
                        break;
                    //case "Garbage":
                    //    root = GetGarbagePath(login);
                    //    break;
                }
            }
            else
                root = file_root;

            string file_path = root + path + fileName;

            src_dir = src_dir.TrimEnd('/');
            src_dir = src_dir.Replace("/", @"\");
            string pth;
            if (src_dir.Length == 0)
                pth = "";
            else
                pth = src_dir + "\\";
            string src_file = root + pth + fileName;

            try
            {
                if (type.Equals("File"))
                {
                    FileInfo fileInf = new FileInfo(file_path);
                    if (fileInf.Exists)
                    {
                        FileInfo past_file;
                        if ((past_file = new FileInfo(src_file)).Exists)
                            past_file.Delete();

                        System.IO.File.Move(file_path, src_file);
                        return;
                    }
                    throw new Exception("Файл не найден.");
                }
                else
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(file_path);
                    if (dirInfo.Exists)
                    {
                        DirectoryInfo past_dir;
                        if ((past_dir = new DirectoryInfo(src_file)).Exists)
                            past_dir.Delete(true);
                        Directory.Move(file_path, src_file);
                        return;
                    }
                    throw new Exception("Каталог не найден.");
                }
            }
            catch { }
        }

        void CopyingFile(string login, string path, string fileName, string type, string action, string src_dir, string file_root)
        {
            string root = null;
            if (file_root == null || file_root.Length == 0)
            {
                switch (action)
                {
                    case "Index":
                        root = GetUserPath(login);
                        break;
                    case "Downloads":
                        root = GetDownloadPath(login);
                        break;
                        //case "Garbage":
                        //    root = GetGarbagePath(login);
                        //    break;
                }
            }
            else
                root = file_root;

            string file_path = root + path + fileName;

            src_dir = src_dir.TrimEnd('/');
            src_dir = src_dir.Replace("/", @"\");
            string pth;
            if (src_dir.Length == 0)
                pth = "";
            else
                pth = src_dir + "\\";
            string src_file = root + pth + fileName;

            try
            {
                if (type.Equals("File"))
                {
                    FileInfo fileInf = new FileInfo(file_path);
                    if (fileInf.Exists)
                    {
                        FileInfo past_file;
                        if ((past_file = new FileInfo(src_file)).Exists)
                            past_file.Delete();

                        System.IO.File.Copy(file_path, src_file);
                        return;
                    }
                    throw new Exception("Файл не найден.");
                }
                else
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(file_path);
                    if (dirInfo.Exists)
                    {
                        DirectoryInfo past_dir;
                        if ((past_dir = new DirectoryInfo(src_file)).Exists)
                            past_dir.Delete(true);
                        Microsoft.VisualBasic.FileIO.FileSystem.CopyDirectory(file_path, src_file);
                        return;
                    }
                    throw new Exception("Каталог не найден.");
                }
            }
            catch { }            
        }

        public void ProcessStorageSpace(int id, string login, PvI_KP_Entities context) 
        {
            var bytes_size = GetDirectorySize(GetUserPath(login));
            var mb_size = (bytes_size / bytes_to_megas);

            context.Users.Find(id).quota_current = (int?)mb_size;
            context.SaveChanges();
        }

        public static long GetDirectorySize(string folderPath)
        {
            DirectoryInfo di = new DirectoryInfo(folderPath);
            return di.EnumerateFiles("*.*", SearchOption.AllDirectories).Sum(fi => fi.Length);
        }

        public static long GetDirectorySize(DirectoryInfo dir)
        {
            return dir.EnumerateFiles("*.*", SearchOption.AllDirectories).Sum(fi => fi.Length);
        }

        public static string GetFileSize(long size)
        {
            int pow = 1024;
            int k = 0;
            double s = size;
            for (; s >= pow;)
            {
                k++;
                s /= pow;
            }

            string metric;
            switch (k) 
            {
                case 1:
                    metric = "КБайт";
                    break;
                case 2:
                    metric = "МБайт";
                    break;
                case 3:
                    metric = "ГБайт";
                    break;
                case 4:
                    metric = "ТБайт";
                    break;
                default:
                    metric = "Байт";
                    break;
            }

            return Math.Round(s, 2) + " " + metric;
        }

        public static string GetPartialPath(string user, string fullpath)
        {
            string users_root = GetUserPath(user);
            string common = string.Concat(users_root.TakeWhile((c, i) => c == fullpath[i]));

            return fullpath.Substring(common.Length);
        }

        static void CopyDirectory(DirectoryInfo source, DirectoryInfo destination)
        {
            if (!destination.Exists)
            {
                destination.Create();
            }

            // Copy all files.
            FileInfo[] files = source.GetFiles();
            foreach (FileInfo file in files)
            {
                file.CopyTo(Path.Combine(destination.FullName,
                    file.Name));
            }

            // Process subdirectories.
            DirectoryInfo[] dirs = source.GetDirectories();
            foreach (DirectoryInfo dir in dirs)
            {
                // Get destination directory.
                string destinationDir = Path.Combine(destination.FullName, dir.Name);

                // Call CopyDirectory() recursively.
                CopyDirectory(dir, new DirectoryInfo(destinationDir));
            }
        }
    }
}