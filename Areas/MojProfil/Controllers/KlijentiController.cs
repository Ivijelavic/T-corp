using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TCorp.Controllers;
using TCorp.EntityFramework;

namespace TCorp.Areas.MojProfil.Controllers {
    public class KlijentiController : ValidUserController {
        public ActionResult Index() {
            var currentUser = authComponent.GetCurrentUser();
            using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                var clients = ctx.Clients.AsNoTracking().Where(c => c.User.Any(u => u.Id == currentUser.Id || u.organization_id == currentUser.organization_id));
                return View(clients.ToList());
            }
        }

        public ActionResult Create() {
            return View();
        }

        [HttpPost]
        public ActionResult Create(string Ime, string Prezime, string Email, string Tvrtka, string Adresa, string KontaktBroj, string OIB) {
            var currentUser = authComponent.GetCurrentUser();
            using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                Client newClient = new Client();
                newClient.Ime = Ime;
                newClient.Prezime = Prezime;
                newClient.Email = Email;
                newClient.Tvrtka = Tvrtka;
                newClient.Adresa = Adresa;
                newClient.KontaktBroj = KontaktBroj;
                newClient.OIB = OIB;
                User user = ctx.Users.Single(u => u.Id == currentUser.Id);
                user.Client.Add(newClient);
                ctx.Clients.Add(newClient);
                ctx.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id) {
            var currentUser = authComponent.GetCurrentUser();
            using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                var client = ctx.Clients.AsNoTracking().SingleOrDefault(c => c.Id == id && c.User.Any(u => u.Id == currentUser.Id));
                if (client == null) {
                    return RedirectToAction("Index");
                }
                else {
                    return View(client);
                }
            }
        }
        [HttpPost]
        public ActionResult Edit(int Id, string Ime, string Prezime, string Email, string Tvrtka, string Adresa, string KontaktBroj, string OIB) {
            var currentUser = authComponent.GetCurrentUser();
            using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                var client = ctx.Clients.SingleOrDefault(c => c.Id == Id && c.User.Any(u => u.Id == currentUser.Id));
                if (client != null) {
                    client.Ime = Ime;
                    client.Prezime = Prezime;
                    client.Email = Email;
                    client.Tvrtka = Tvrtka;
                    client.Adresa = Adresa;
                    client.KontaktBroj = KontaktBroj;
                    client.OIB = OIB;
                    ctx.SaveChanges();
                }
                return RedirectToAction("Index");
            }
        }

        public ActionResult Details(int id) {
            var currentUser = authComponent.GetCurrentUser();
            using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                Client client = ctx.Clients.SingleOrDefault(c => c.Id == id && c.User.Any(u => u.Id == currentUser.Id));
                if (client == null) {
                    return RedirectToAction("Index");
                }
                else {
                    return View(client);
                }
            }
        }

        public ActionResult Delete(int id) {
            throw new NotImplementedException();
            var currentUser = authComponent.GetCurrentUser();
            using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                Client client = ctx.Clients.SingleOrDefault(c => c.Id == id && c.User.Any(u => u.Id == currentUser.Id));
                if (client != null) {
                    ctx.Clients.Remove(client);
                    ctx.SaveChanges();
                }
            }
            return RedirectToAction("Index");
        }

        public ActionResult Develop1() {
            return View();
        }

        public ActionResult Develop2() {
            return View();
        }
    }
}
