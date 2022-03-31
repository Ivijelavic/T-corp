using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TCorp.Components;
using TCorp.Controllers;
using TCorp.EntityFramework;

namespace TCorp.Areas.ControlPanel.Controllers {
    public class UlogeController : SuperAdminController {

        private RoleComponent rc = new RoleComponent();

        public ActionResult Index() {
            using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                var roles = ctx.Roles.AsNoTracking().ToList();
                return View(roles);
            }
        }

        public ActionResult Details(int id = 0) {
            using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                Role role = ctx.Roles.AsNoTracking().SingleOrDefault(r => r.Id == id);
                if (role == null) {
                    return HttpNotFound();
                }
                int roleId = authComponent.GetCurrentUser().Role.Id;
                ViewBag.CategoriesTable = rc.CreateHtmlTable(roleId, true);
                return View(role);
            }
        }

        public ActionResult Create() {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Role role) {
            using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                //validacija
                ctx.Roles.Add(role);
                ctx.SaveChanges();
                return RedirectToAction("Index");

            }
        }

        public ActionResult Edit(int Id = 0) {
            using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                Role role = ctx.Roles.AsNoTracking().SingleOrDefault(r => r.Id == Id);
                if (role == null) {
                    return HttpNotFound();
                }
                ViewBag.CategoriesTable = rc.CreateHtmlTable(role.Id);
                return View(role);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int Id, string Name, bool ManagerPrivilages, List<int> Visible) {
            using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                //validacija
                Role role = ctx.Roles.SingleOrDefault(r => r.Id == Id);
                if (role == null) {
                    return HttpNotFound("Invalid id");
                }
                //role.Update(Name, MaxDiscount);
                role.Name = Name;
                role.ManagerPrivilages = ManagerPrivilages;
                ctx.Database.ExecuteSqlCommand("DELETE FROM Category_Role WHERE role_id = '" + role.Id + "'");
                foreach (var categoryId in Visible) {
                    Category c = ctx.Categories.Single(ca => ca.Id == categoryId);
                    role.Category.Add(c);
                }
                ctx.Database.ExecuteSqlCommand("DELETE FROM Category_Role_MaxDiscount WHERE role_id = '" + role.Id + "'");
                string[] discountIndex = Request.Form.GetValues("Discount.Index");
                foreach (var categoryId in discountIndex) {
                    Category_Role_MaxDiscount maxDisount = new Category_Role_MaxDiscount();
                    maxDisount.category_id = int.Parse(categoryId);
                    maxDisount.role_id = role.Id;
                    maxDisount.MaxDiscount = double.Parse(Request.Form[String.Format("Discount[{0}].Value", categoryId)]) / 100;
                    role.Category_Role_MaxDiscount.Add(maxDisount);
                }
                ctx.SaveChanges();
                return RedirectToAction("Index");
            }
        }

        public ActionResult Delete(int id = 0) {
            using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                Role role = ctx.Roles.Find(id);
                if (role == null) {
                    return HttpNotFound();
                }
                ctx.Roles.Remove(role);
                ctx.SaveChanges();
                return RedirectToAction("Index");
            }
        }
    }
}