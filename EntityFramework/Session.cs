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
    
    public partial class Session
    {
        public int user_id { get; set; }
        public string AccessToken { get; set; }
        public System.DateTime Created { get; set; }
        public Nullable<System.DateTime> ValidUntil { get; set; }
    
        public virtual User Owner { get; set; }
    }
}