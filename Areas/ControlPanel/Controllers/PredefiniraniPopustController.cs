using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using TCorp.EntityFramework;
using TCorp.Controllers;

namespace TCorp.Areas.ControlPanel.Controllers {
    /* SuperAdminController */
    public class PredefiniraniPopustController : SuperAdminController {

        public ActionResult Index() {
            using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                var discounts = ctx.PredefinedDiscount
                    .Include(pp => pp.Contract)
                    .AsNoTracking()
                    .ToList();
                return View(discounts);
            }
        }

        public ActionResult Details(int id = 0) {
            return View();
        }

        public ActionResult Create() {
            using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                ViewBag.ContractId = new SelectList(ctx.Contract.AsNoTracking().ToList(), "Id", "Name");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(string Name, int DurationInMonths, int ContractId) {
            using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                PredefinedDiscount pd = new PredefinedDiscount();
                pd.Name = Name;
                pd.DurationInMonths = DurationInMonths;
                pd.contract_id = ContractId;
                ctx.PredefinedDiscount.Add(pd);
                ctx.SaveChanges();
            }
            return RedirectToAction("Index");
        }
        public ActionResult Visibility(int id) {
            using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                PredefinedDiscount pd = ctx.PredefinedDiscount.Include(pp => pp.Role).AsNoTracking().SingleOrDefault(p => p.Id == id);
                if (pd == null) {
                    return new HttpNotFoundResult("Invalid id");
                }
                ViewBag.AllRoles = ctx.Roles.AsNoTracking().ToList();
                return View(pd);
            }
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Visibility(int Id, List<bool> IsVisible, List<int> Role) {
            using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                PredefinedDiscount pd = ctx.PredefinedDiscount.Include(pp => pp.Role).SingleOrDefault(p => p.Id == Id);
                if (pd == null) {
                    return new HttpNotFoundResult("Invalid id");
                }
                var rolesLookup = ctx.Roles.ToList();
                //ctx.Database.ExecuteSqlCommand("DELETE FROM PredefinedDiscount_Role WHERE predefinedDiscount_id = '" + Id + "'");
                pd.Role.Clear();
                ctx.SaveChanges();
                for (int i = 0; i < Role.Count; i++) {
                    if (IsVisible[i] == true) {
                        Role r = rolesLookup.Single(role => role.Id == Role[i]);
                        pd.Role.Add(r);
                    }
                }
                ctx.SaveChanges();
                return RedirectToAction("Index");
            }
        }

        public ActionResult Edit(int id) {
            using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                PredefinedDiscount pd = ctx.PredefinedDiscount.AsNoTracking().SingleOrDefault(p => p.Id == id);
                if (pd == null) {
                    return new HttpNotFoundResult("Invalid id");
                }
                ViewBag.ContractId = new SelectList(ctx.Contract.AsNoTracking().ToList(), "Id", "Name", pd.contract_id);
                return View(pd);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int Id, string Name, int DurationInMonths, int ContractId) {
            using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                PredefinedDiscount pd = ctx.PredefinedDiscount.SingleOrDefault(p => p.Id == Id);
                if (pd == null) {
                    return new HttpNotFoundResult("Invalid id");
                }
                pd.Name = Name;
                pd.DurationInMonths = DurationInMonths;
                pd.contract_id = ContractId;
                ctx.SaveChanges();
                return RedirectToAction("Index");
            }
        }

        public ActionResult Delete(int id = 0) {
            using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                PredefinedDiscount pp = ctx.PredefinedDiscount.Find(id);
                if (pp == null) {
                    return HttpNotFound("Invalid id");
                }
                ctx.PredefinedDiscount.Remove(pp);
                ctx.SaveChanges();
                return RedirectToAction("Index");
            }
        }
    }
}
