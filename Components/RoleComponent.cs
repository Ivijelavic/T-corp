using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TCorp.EntityFramework;

namespace TCorp.Components {
    public class RoleComponent {
        public string CreateHtmlTable(int roleId, bool disabled = false) {
            string html = String.Empty;
            string disabledHtml = (disabled == true) ? @"disabled=""disabled""" : String.Empty;
            using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                html += @"<div id=""tbl"" class=""well"" style=""width:940px; padding: 8px 0;"">";
                html += @"<div style=""overflow-y: scroll; overflow-x: hidden; height: 500px;"">";
                html += @"<ul class=""nav nav-list"">";
                var sveKategorije = ctx.Categories.AsNoTracking().ToList();
                foreach (Category child in sveKategorije.Where(k => k.ParentId == null).OrderBy(k => k.Name)) {
                    string isChecked = child.AuthorizedRoles.Any(role => role.Id == roleId) ? @"checked=""checked""" : String.Empty;
                    string isVisibleCheckbox = String.Format(@"<input class=""visibilityCheckbox"" name=""Visible"" type=""checkbox"" value=""{0}"" {1} {2}>", child.Id, isChecked, disabledHtml);
                    var discountEntity = child.Category_Role_MaxDiscount.SingleOrDefault(d => d.role_id == roleId);
                    string hidden = String.Format(@"<input type=""hidden"" name=""Discount.Index"" value=""{0}"">", child.Id);
                    string discountTextbox = String.Format(@"{0}<input class=""discountText"" name=""Discount[{1}].Value"" type=""text"" value=""{2}"" {3}>", hidden, child.Id, discountEntity == null ? 0 : 100 * discountEntity.MaxDiscount, disabledHtml);
                    if (child.ChildCategories.Count == 0) {
                        html += String.Format(@"<li><a class=""link-toggler"" href=""javascript:void(0)"">{0}<br>Vidljivo: {1}<br>Popust: {2}</a></li>", child.Name, isVisibleCheckbox, discountTextbox);
                    }
                    else {
                        string children = CategoryChildren(roleId, child, sveKategorije, disabled);
                        html += String.Format(@"<li><a class=""link-toggler"" href=""javascript:void(0)""><label class=""nav-header"">{0}</label>Vidljivo: {1}<br>Popust: {2}</a>{3}</li>", child.Name, isVisibleCheckbox, discountTextbox, children);
                    }
                }
                html += @"</ul>";
                html += @"</div>";
                html += @"</div>";
            }
            return html;
        }

        private string CategoryChildren(int roleId, Category c, List<Category> sveKategorije, bool disabled = false) {
            string result = @"<ul class=""nav nav-list tree"">";
            string disabledHtml = (disabled == true) ? @"disabled=""disabled""" : String.Empty;
            var childKategorije = sveKategorije.Where(k => k.ParentId == c.Id).OrderBy(k => k.Name).ToList();
            foreach (Category child in childKategorije) {
                string isChecked = child.AuthorizedRoles.Any(role => role.Id == roleId) ? @"checked=""checked""" : String.Empty;
                string isVisibleCheckbox = String.Format(@"<input class=""visibilityCheckbox"" name=""Visible"" type=""checkbox"" value=""{0}"" {1} {2}>", child.Id, isChecked, disabledHtml);
                var discountEntity = child.Category_Role_MaxDiscount.SingleOrDefault(d => d.role_id == roleId);
                string hidden = String.Format(@"<input type=""hidden"" name=""Discount.Index"" value=""{0}"">", child.Id);
                string discountTextbox = String.Format(@"{0}<input class=""discountText"" name=""Discount[{1}].Value"" type=""text"" value=""{2}"" {3}>", hidden, child.Id, discountEntity == null ? 0 : 100 * discountEntity.MaxDiscount, disabledHtml);
                if (child.ChildCategories.Count == 0) {
                    result += String.Format(@"<li><a class=""link-toggler"" href=""javascript:void(0)"">{0}<br>Vidljivo: {1}<br>Popust: {2}</a></li>", child.Name, isVisibleCheckbox, discountTextbox);
                }
                else {
                    string children = CategoryChildren(roleId, child, sveKategorije, disabled);
                    result += String.Format(@"<li><a class=""link-toggler"" href=""javascript:void(0)""><label class=""nav-header"">{0}</label>Vidljivo: {1}<br>Popust: {2}</a>{3}</li>", child.Name, isVisibleCheckbox, discountTextbox, children);
                }
            }
            result += "</ul>";
            return result;
        }
    }
}