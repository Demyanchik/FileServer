using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication2.Classes
{
    public class UsersInGroup
    {
        public int userId { get; set; }
        public string fullname {get; set;}
        public bool isMember{ get; set; }

        public bool writable { get; set; }

    }
}