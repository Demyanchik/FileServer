using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication2.Classes
{
    public class UserInfo
    {
        public string login { get; set; }
        public string password { get; set; }
        public int quota_current { get; set; }
        public int quota_max { get; set; }
        public string fullname { get; set; }
        public string role { get; set; }
    }
}