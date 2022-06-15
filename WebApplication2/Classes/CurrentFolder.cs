using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

using WebApplication2.Controllers;

namespace FileSystem
{
    public class CurrentFolder
    {
        public static string icons_dir = @"E:\Labs\Labs_4_1\PvI_KP\FileIcons\icons\48px\";
        public static string icons_dir512 = @"E:\Labs\Labs_4_1\PvI_KP\FileIcons\icons\512px\";


        string path;
        List<DirectoryInfo> subdirs;
        List<FileInfo> files;
        string user;

        //common + username
        List<DirectoryInfo> GetDirs(string path)
        {
            DirectoryInfo main_folder = new DirectoryInfo(path);
            if (main_folder.Exists)
                return main_folder.GetDirectories().ToList();

            throw new Exception("Каталог не существует.");
        }
        List<FileInfo> GetFiles(string path)
        {
            DirectoryInfo main_folder = new DirectoryInfo(path);
            if (main_folder.Exists)
                return main_folder.GetFiles().ToList();

            throw new Exception("Каталог не существует.");
        }

        public static FileInfo GetFileIcon(string path)
        {
            FileInfo fileInf = new FileInfo(path);

            string ext = fileInf.Extension.Replace(".", "");
            string file_icon_name = ext + ".png";
            FileInfo file_icon = new FileInfo(icons_dir + file_icon_name);

            if (file_icon.Exists)
            {
                return file_icon;
            }
            else
            {
                return new FileInfo(icons_dir + "_blank.png");
            }

        }
        public CurrentFolder(string user, string path)
        {
            this.user = user;

            this.path = path;
            subdirs = GetDirs(path);
            files = GetFiles(path);
        }
        public static FileInfo GetFolderIcon() 
        {
            return new FileInfo(icons_dir512 + "folder.png");
        }
        public List<Item> GetAll()
        {
            List<Item> items = new List<Item>();

            foreach (var dir in subdirs)
            {
                var item = new Item(new FileInfo(icons_dir + "folder.png"), dir.Name, "Folder", GetPartialPath(user, dir.FullName));
                item.IsSharedLink = HomeController.IsSharedLink(user, item.Partial_path);
                item.SharedLink = HomeController.GetSharedLink(user, item.Partial_path);

                item.Size = HomeController.GetFileSize(HomeController.GetDirectorySize(dir));
                item.LastChange = dir.LastWriteTime;

                items.Add(item);
            }

            foreach (var file in files)
            {
                var item = new Item(GetFileIcon(file.FullName), file.Name, "File", GetPartialPath(user, file.FullName));
                item.IsSharedLink = HomeController.IsSharedLink(user, item.Partial_path);
                item.SharedLink = HomeController.GetSharedLink(user, item.Partial_path);

                item.Size = HomeController.GetFileSize(file.Length);
                item.LastChange = file.LastWriteTime;

                items.Add(item);

            }

            return items;
        }

        string GetPartialPath(string user, string fullpath) 
        {
            return HomeController.GetPartialPath(user, fullpath).Replace(@"\", "/");
        }
    }

    public class Item
    {
        public FileInfo Icon { get; set; }
        public string Partial_path { get; set; }

        public bool IsSharedLink { get; set; }

        public string SharedLink { get; set; }

        public string Type { get; set; }

        public string Size { get; set; }

        public DateTime LastChange{ get; set; }


        string name;
        public string Name 
        {
            get { return name; }
            set 
            {
                name = value;

                if (value.Length >= 22)
                    ShortedName = value.Substring(0, 13) + "..." + value.Substring(value.Length - 6, 6);
                else
                    ShortedName = value;

                ShortedName = ShortedName.Replace('—', '-');
            }
        }
        public string ShortedName { get; set; }
        public Item(FileInfo icon, string name, string type, string path)
        {
            Icon = icon;
            Name = name;
            Type = type;
            Partial_path = path;
        }

        public Item() { }
    }
}