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
    
    public partial class Client
    {
        public Client()
        {
            this.Receipt = new HashSet<Receipt>();
            this.User = new HashSet<User>();
        }
    
        public int Id { get; set; }
        public string Email { get; set; }
        public string Ime { get; set; }
        public string Prezime { get; set; }
        public string Tvrtka { get; set; }
        public string OIB { get; set; }
        public string Adresa { get; set; }
        public string KontaktBroj { get; set; }
    
        public virtual ICollection<Receipt> Receipt { get; set; }
        public virtual ICollection<User> User { get; set; }
    }
}
