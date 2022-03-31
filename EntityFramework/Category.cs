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
    
    public partial class Category
    {
        public Category()
        {
            this.ChildCategories = new HashSet<Category>();
            this.AuthorizedRoles = new HashSet<Role>();
            this.ReceiptTemplateBasket = new HashSet<ReceiptTemplateBasket>();
            this.Category_Role_MaxDiscount = new HashSet<Category_Role_MaxDiscount>();
            this.AdditionalOptions = new HashSet<AdditionalOptions>();
            this.Category_Price = new HashSet<Category_Price>();
            this.ReceiptBasket = new HashSet<ReceiptBasket>();
            this.PredefinedDiscount_Category = new HashSet<PredefinedDiscount_Category>();
            this.Device_Price = new HashSet<Device_Price>();
            this.Device_TSpotApps_Price = new HashSet<Device_TSpotApps_Price>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public Nullable<int> ParentId { get; set; }
        public bool IsHidden { get; set; }
        public bool IsDeleted { get; set; }
        public int Sort { get; set; }
        public string Description { get; set; }
        public string Info { get; set; }
        public Nullable<int> InitialQuantity { get; set; }
        public Nullable<decimal> InitialDiscount { get; set; }
        public string OfferName { get; set; }
        public string OfferNameOneTime { get; set; }
        public bool ShowContractOfferName { get; set; }
    
        public virtual ICollection<Category> ChildCategories { get; set; }
        public virtual Category ParentCategory { get; set; }
        public virtual ICollection<Role> AuthorizedRoles { get; set; }
        public virtual ICollection<ReceiptTemplateBasket> ReceiptTemplateBasket { get; set; }
        public virtual ICollection<Category_Role_MaxDiscount> Category_Role_MaxDiscount { get; set; }
        public virtual ICollection<AdditionalOptions> AdditionalOptions { get; set; }
        public virtual ICollection<Category_Price> Category_Price { get; set; }
        public virtual ICollection<ReceiptBasket> ReceiptBasket { get; set; }
        public virtual ICollection<PredefinedDiscount_Category> PredefinedDiscount_Category { get; set; }
        public virtual ICollection<Device_Price> Device_Price { get; set; }
        public virtual Device_SelectedCategories Device_SelectedCategories { get; set; }
        public virtual Category_InStock Category_InStock { get; set; }
        public virtual ICollection<Device_TSpotApps_Price> Device_TSpotApps_Price { get; set; }
        public virtual Device_TSpotApps_SelectedCategories Device_TSpotApps_SelectedCategories { get; set; }
    }
}
