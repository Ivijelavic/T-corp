//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TCorp.EntityFramework
{
    using System;
    using System.Collections.Generic;
    
    public partial class Category_Role_MaxDiscount
    {
        public int category_id { get; set; }
        public int role_id { get; set; }
        public double MaxDiscount { get; set; }
    
        public virtual Category Category { get; set; }
        public virtual Role Role { get; set; }
    }
}
