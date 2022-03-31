using System;
using System.Data.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TCorp.Components;
using TCorp.EntityFramework;
using TCorp.Controllers;

namespace TCorp.Areas.ControlPanel.Controllers {
    public class OrganizacijeController : SuperAdminController {
        private OrganizationComponent organizationComponent = new OrganizationComponent();
        public ActionResult Index(int? id = null) {
            List<Organization> childCategories = null;
            int orgLevel;
            using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                if (id == null) {
                    childCategories = ctx.Organization.Where(o => o.parent_id == null).ToList();
                    orgLevel = 1;
                }
                else {
                    Organization organization = ctx.Organization.SingleOrDefault(o => o.Id == id);
                    if (organization == null) {
                        return new HttpNotFoundResult("Invalid id");
                    }
                    else {
                        childCategories = organization.ChildOrganizations.ToList();
                        orgLevel = organizationComponent.GetLevel(organization);
                    }
                }
            }
            ViewBag.Mrvice = organizationComponent.GetAncestorsLinks(id);
            ViewBag.CreateId = id;
            ViewBag.CanCreate = (orgLevel < 6);
            return View(childCategories);
        }

        public ActionResult Create(int? parentId = null) {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Create(string Name, int? parentId) {
            /* validacija, ne smije ić dublje od razine 6 */
            using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                Organization o = new Organization();
                o.Name = Name;
                o.parent_id = parentId;
                o.IsDeleted = false;
                ctx.Organization.Add(o);
                ctx.SaveChanges();
                return RedirectToAction("Index", new { id = parentId });
            }
        }

        [ValidateInput(false)]
        public ActionResult Edit(int id = 0) {
            if (id == 0) {
                return RedirectToAction("Index");
            }
            else {
                using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                    Organization organization = ctx.Organization.SingleOrDefault(o => o.Id == id);
                    if (organization == null) {
                        return new HttpNotFoundResult("Invalid id");
                    }
                    else {
                        return View(organization);
                    }
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(int Id, string Name, bool IsDeleted) {
            using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                Organization editedOrganization = ctx.Organization.SingleOrDefault(o => o.Id == Id);
                if (editedOrganization == null) {
                    return new HttpNotFoundResult("Invalid id");
                }
                editedOrganization.Name = Name;
                editedOrganization.IsDeleted = IsDeleted;
                ctx.SaveChanges();
                return RedirectToAction("Index", new { id = editedOrganization.parent_id });
            }
        }

        public ActionResult Delete(int id = 0) {
            if (id == 0) {
                return RedirectToAction("Index");
            }
            else {
                using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                    Organization organization = ctx.Organization.SingleOrDefault(o => o.Id == id);
                    if (organization == null) {
                        return new HttpNotFoundResult("Invalid id");
                    }
                    else {
                        int? parentId = organization.parent_id;
                        organization.IsDeleted = true;
                        ctx.SaveChanges();
                        return RedirectToAction("Index", new { id = parentId });
                    }
                }
            }
        }
    }
}
