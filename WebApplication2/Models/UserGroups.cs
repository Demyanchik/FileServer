//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WebApplication2.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class UserGroups
    {
        public int id { get; set; }
        public Nullable<int> user_id { get; set; }
        public string user_group { get; set; }
        public Nullable<bool> C_writable { get; set; }
    
        public virtual Groups Groups { get; set; }
        public virtual Users Users { get; set; }
    }
}
