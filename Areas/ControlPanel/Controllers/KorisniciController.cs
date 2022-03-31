using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TCorp.Areas.ControlPanel.Models;
using TCorp.Components;
using TCorp.Controllers;
using TCorp.EntityFramework;

namespace TCorp.Areas.ControlPanel.Controllers {
    public class KorisniciController : SuperAdminController {
        private AuthComponent login = new AuthComponent();
        private OrganizationComponent orgComponent = new OrganizationComponent();
        public ActionResult Index() {
            using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                var users = ctx.Users.AsNoTracking().Include(u => u.Role).Include(u => u.Organization);
                return View(users.ToList());
            }
        }

        public ActionResult Details(int id = 0) {
            using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                User user = ctx.Users.AsNoTracking().Include(u => u.Role).Include(u => u.Organization).SingleOrDefault(u => u.Id == id);
                if (user == null) {
                    return HttpNotFound("Invalid id");
                }
                return View(user);
            }
        }

        public ActionResult Create() {
            using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                ViewBag.RoleId = new SelectList(ctx.Roles.AsNoTracking().ToList(), "Id", "Name");
                ViewBag.OrgId = orgComponent.GetSelectList();
                ViewBag.SuperiorId = new SelectList(ctx.Users.AsNoTracking().ToList(), "Id", "DisplayName");
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(NewUserViewModel user) {
            using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                User newUser = new User();
                if (user.ValidateCreate(TempData) == true) {
                    newUser.Load(user);
                    ctx.Users.Add(newUser);
                    ctx.SaveChanges();
                    return RedirectToAction("Index");
                }
                else {
                    ViewBag.RoleId = new SelectList(ctx.Roles.AsNoTracking().ToList(), "Id", "Name");
                    ViewBag.OrgId = orgComponent.GetSelectList();
                    ViewBag.SuperiorId = new SelectList(ctx.Users.AsNoTracking().ToList(), "Id", "DisplayName");
                    return View();
                }
            }
        }

        public ActionResult Edit(int id = 0) {
            using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                User user = ctx.Users.AsNoTracking().Include(u => u.Role).Include(u => u.Organization).SingleOrDefault(u => u.Id == id);
                if (user == null) {
                    return HttpNotFound();
                }
                ViewBag.RoleId = new SelectList(ctx.Roles.AsNoTracking().ToList(), "Id", "Name", user.role_id);
                ViewBag.OrgId = orgComponent.GetSelectList(user.organization_id);
                ViewBag.SuperiorId = new SelectList(ctx.Users.AsNoTracking().ToList(), "Id", "DisplayName", user.superiorUser_id);
                return View(user);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(NewUserViewModel user) {
            using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                if (user.ValidateUpdate(TempData) == true) {
                    User editedUser = ctx.Users.Find(user.Id);
                    if (editedUser == null) {
                        return HttpNotFound("Invalid id");
                    }
                    editedUser.Update(user);
                    ctx.SaveChanges();
                    return RedirectToAction("Index");
                }
                ViewBag.RoleId = new SelectList(ctx.Roles.AsNoTracking().ToList(), "Id", "Name", user.role_id);
                ViewBag.OrgId = orgComponent.GetSelectList(user.organization_id);
                ViewBag.SuperiorId = new SelectList(ctx.Users.AsNoTracking().ToList(), "Id", "DisplayName", user.superiorUser_id);
                return View(user);
            }
        }

        public ActionResult Delete(int id = 0) {
            using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                User user = ctx.Users.Find(id);
                if (user == null) {
                    return HttpNotFound("Invalid id");
                }
                ctx.Users.Remove(user);
                ctx.SaveChanges();
                return RedirectToAction("Index");
            }
        }

        public ActionResult EraseToken(int id = 0) {
            using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                User user = ctx.Users.Include(u => u.Session).SingleOrDefault(u => u.Id == id);
                if (user == null) {
                    return HttpNotFound("Invalid id");
                }
                if (user.Session != null) {
                    ctx.Sessions.Remove(user.Session);
                    ctx.SaveChanges();
                }
                return View();
            }
        }

        public ActionResult ResetLoginAttempts(int id = 0) {
            using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                User user = ctx.Users.SingleOrDefault(u => u.Id == id);
                if (user == null) {
                    return HttpNotFound("Invalid id");
                }
                user.FailedLoginAttempts = 0;
                ctx.SaveChanges();
                return RedirectToAction("Details", new { id = id });
            }
        }
    }
}