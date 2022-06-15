using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApplication2.Controllers
{
    public partial class HomeController : Controller
    {
        List<BackPage> GetBackPages(string catchall, int flag) 
        {
            string root_page = null;
            string root_url = null;

            switch (flag) 
            {
                case 1:
                    root_page = "Файлы";
                    root_url = hostname + "Home/Index/";
                    break;
                case 2:
                    root_page = "Загрузки";
                    root_url = hostname + "Home/Downloads/";
                    break;
                case 3:
                    root_page = "Корзина";
                    root_url = hostname + "Home/Garbage/";
                    break;
            }

            List<BackPage> list = new List<BackPage>();
            var dirs = catchall.Split('/');
            list.Add(new BackPage { Name = root_page, Url = root_url });

            for (int i = 0; i < dirs.Length - 2; i++) 
            {
                BackPage page = new BackPage();
                page.Name = dirs[i];

                string path = "";
                for (int j = -1; j < i; j++)
                {
                    path += dirs[j + 1] + "/";
                }

                page.Url = root_url + path;

                list.Add(page);
            }

            return list;
        }

        List<BackPage> GetBackPages(string catchall, string root_page, string root_url)
        {
            List<BackPage> list = new List<BackPage>();
            var dirs = catchall.Split('/');
            list.Add(new BackPage { Name = root_page, Url = root_url });

            for (int i = 0; i < dirs.Length - 2; i++)
            {
                BackPage page = new BackPage();
                page.Name = dirs[i];

                string path = "";
                for (int j = -1; j < i; j++)
                {
                    path += dirs[j + 1] + "/";
                }

                page.Url = root_url + path;

                list.Add(page);
            }

            return list;
        }

        public string GetCurrentPage(string catchall) 
        {
            var dirs = catchall.Split('/');
            return dirs[dirs.Length - 2];
        }
    }

    public class BackPage
    {
        public string Name { get; set; }
        public string Url { get; set; }
    }
}