using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication2.Models;
using WebApplication2.Classes;

namespace WebApplication2.Controllers
{
    public partial class HomeController : Controller
    {
        List<Users> GetOpenUsers() 
        {
            var users = from usr in context.Users
                        where usr.allow_add == true
                        select usr;

            return users.ToList();
        }

        List<Users> GetOpenUsersExceptOwner(string owner)
        {
            var users = from usr in context.Users
                        where usr.allow_add == true
                        && !usr.login.Equals(owner)
                        select usr;

            return users.ToList();
        }

        List<Groups> GetUsersGroups(int id)
        {
            var groups = from grp in context.UserGroups
                         join grp_info in context.Groups on grp.user_group equals grp_info.group_name
                         where grp.user_id == id
                         select grp_info;

            return groups.ToList();
        }

        bool isInUsersGroups(List<UsersInGroup> usr_grp, int id) 
        {
            foreach (var usr in usr_grp) 
            {
                if (usr.userId == id)
                    return true;
            }

            return false;
        }
        List<UsersInGroup> GetUsersViewByGroup(string group_name)
        {
            List<UsersInGroup> nowInGroup;
            var now_in_group = from usr_grp in context.UserGroups
                               join usr in context.Users on usr_grp.user_id equals usr.id
                               where usr_grp.user_group.Equals(group_name)
                               select new UsersInGroup
                               {
                                   userId = usr.id,
                                   fullname = usr.fullname,
                                   isMember = true,
                                   writable = usr_grp.C_writable == null ? false: (bool)usr_grp.C_writable
                               };

            nowInGroup = now_in_group.ToList();
            List<UsersInGroup> notInGroup = new List<UsersInGroup>();

            var open_users = GetOpenUsers();

            foreach (var usr in open_users)
            {
                if (!isInUsersGroups(nowInGroup, usr.id))
                {
                    notInGroup.Add(new UsersInGroup { userId = usr.id, fullname = usr.fullname, isMember = false, writable =false});
                }
            }

            var result = nowInGroup.Union(notInGroup);

            return result.ToList();
        }

        void CreateGroup_FileSystem()
        {
            string name = Request.Form["name"];
            if (!Directory.Exists(shared + name))
            {
                Directory.CreateDirectory(shared + name);
                //DirectoryCopy(@"E:\MyCloud\new_user_template", GetUserPath(login), true);
            }
        }
        void CreateGroup_DB(Users our_user)
        {
            Groups grp = new Groups();
            grp.group_name = Request.Form["name"];
            grp.created = DateTime.Now;
            grp.owner_id = our_user.id;

            context.Groups.Add(grp);
            context.SaveChanges();

            int owner_id = int.Parse(Request.Form["owner_id"]);
            context.UserGroups.Add(new UserGroups { user_id = owner_id, user_group = grp.group_name, C_writable = true });

            int count = int.Parse(Request.Form["users_count"]);
            for (int i = 1; i <= count; i++)
            {
                string user = null;
                if ((user = Request.Form["user" + i]) != null)
                {
                    context.UserGroups.Add(new UserGroups { user_id = int.Parse(user), user_group = grp.group_name, C_writable = false });
                }
            }
            context.SaveChanges();
        }

        void DeleteGroup_FileSystem(string group_name)
        {
            try
            {
                Directory.Delete(shared + group_name,  true);
            }
            catch
            {

            }

        }
        void DeleteGroup_DB(string group_name)
        {
            var group = context.Groups.Find(group_name);

            context.Groups.Remove(group);

            context.SaveChanges();
        }

        void TryUpdateGroup(string group_name)
        {
            //List<UserGroups> remove_users = new List<UserGroups>();
            //foreach (var usr in context.UserGroups) 
            //{
            //    if (usr.user_group.Equals(group_name))
            //        remove_users.Add(usr);
            //}
            //context.UserGroups.RemoveRange(remove_users);
            //context.SaveChanges();

            var grp = context.Groups.Find(group_name);
            var created = grp.created;
            var owner_id = grp.owner_id;

            context.Groups.Remove(grp);
            context.SaveChanges();

            //Переименование папки
            var new_name = Request.Form["new_name"];

            try
            {
                if (!group_name.Equals(new_name))
                    System.IO.Directory.Move(shared + group_name, shared + new_name);
            }
            catch 
            {

            }

            Groups group = new Groups();
            group.created = created;
            group.owner_id = owner_id;
            group.group_name = new_name;

            context.Groups.Add(group);
            context.SaveChanges();

            int users_count = int.Parse(Request.Form["users_count"]);
            for (int i = 1; i <= users_count; i++) 
            {
                string user = null;
                if ((user = Request.Form["user" + i]) != null)
                {
                    context.UserGroups.Add(new UserGroups { user_id = int.Parse(user), user_group = group.group_name, C_writable = WriteMode(Request.Form["write_mode" + i.ToString()])});
                }
            }

            context.SaveChanges();
        }

        bool WriteMode(string option)
        {
            switch (option)
            {
                case "Чтение":
                    return false;
                case "Чтение/Запись":
                    return true;
            }

            return false;
        }

        public static string getOwnerName(PvI_KP_Entities context, int? id) 
        {
            return id!=null?context.Users.Find(id).fullname:user404;
        }
    }
}