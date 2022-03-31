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
    
    public partial class PredefinedDiscount
    {
        public PredefinedDiscount()
        {
            this.PredefinedDiscount_Category = new HashSet<PredefinedDiscount_Category>();
            this.Role = new HashSet<Role>();
            this.ReceiptTemplateBasket = new HashSet<ReceiptTemplateBasket>();
            this.ReceiptBasket = new HashSet<ReceiptBasket>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public int contract_id { get; set; }
        public string Description { get; set; }
        public Nullable<int> DurationInMonths { get; set; }
    
        public virtual Contract Contract { get; set; }
        public virtual ICollection<PredefinedDiscount_Category> PredefinedDiscount_Category { get; set; }
        public virtual ICollection<Role> Role { get; set; }
        public virtual ICollection<ReceiptTemplateBasket> ReceiptTemplateBasket { get; set; }
        public virtual ICollection<ReceiptBasket> ReceiptBasket { get; set; }
    }
}