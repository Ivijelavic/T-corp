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
    
    public partial class ReceiptDefaultOption
    {
        public int Id { get; set; }
        public int receiptTemplateBasket_id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public Nullable<decimal> Price { get; set; }
        public Nullable<int> Index { get; set; }
    
        public virtual ReceiptTemplateBasket ReceiptTemplateBasket { get; set; }
    }
}
