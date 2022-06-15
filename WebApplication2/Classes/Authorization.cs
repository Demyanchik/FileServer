using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApplication2.Controllers
{
    public partial class HomeController: Controller
    {
        bool AuthorizationCheck()
        {
            if (Session["login"] == null) 
            {
                if (Request.Cookies["login"] != null)
                {
                    if (Request.Cookies["login"].Value == "")
                    {
                        return false;
                    }
                }
                else 
                {
                    return false;
                }
            }

            return true;
        }
        void GetCredentials(out int id, out string login, out string password)
        {
            string user = (Session["login"] == null ? Request.Cookies["login"].Value : Session["login"].ToString());
            login = user;
            password = (Session["password"] == null ? Request.Cookies["password"].Value : Session["password"].ToString());
            id = GetIdByLogin(user);

        }

        static int GetIdByLogin(string login) 
        {
            return (from users in context.Users
                         where users.login.Equals(login)
                         select users).ToList()[0].id;
        }

        void GetCredentials(out string login)
        {
            login = (Session["login"] == null ? Request.Cookies["login"].Value : Session["login"].ToString());
        }

        void SetSession(string key, string value)
        {
            Session[key] = value;
        }

        void SetCookie(string key, string value)
        {
            Response.Cookies[key].Value = value;
        }
    }
}