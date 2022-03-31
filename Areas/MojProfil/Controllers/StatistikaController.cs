using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TCorp.Components;
using TCorp.Controllers;
using TCorp.JsonResponseModels;

namespace TCorp.Areas.MojProfil.Controllers {
    public class StatistikaController : ValidUserController {
        StatisticsComponent sc = StatisticsComponent.Instance;
        public ActionResult Index() {
            var currentUser = authComponent.GetCurrentUser();
            var test = sc.StatisticsContract(currentUser);
            List<KeyValuePair<int, string>> supportedStatistics = new List<KeyValuePair<int, string>>();
            List<KeyValuePair<int, string>> supportedCategories = new List<KeyValuePair<int, string>>();
            List<KeyValuePair<string, string>> clients = new List<KeyValuePair<string, string>>();
            foreach (var option in test.SupportedStatistics) {
                KeyValuePair<int, string> kvp = new KeyValuePair<int, string>(option.Id, option.Name);
                supportedStatistics.Add(kvp);
            }
            foreach (var option in test.InitialCategories) {
                KeyValuePair<int, string> kvp = new KeyValuePair<int, string>(option.CategoryId, option.Name);
                supportedCategories.Add(kvp);
            }
            foreach (var option in test.Clients) {
                KeyValuePair<string, string> kvp = new KeyValuePair<string, string>(option.Id, option.DisplayName);
                clients.Add(kvp);
            }
            ViewBag.SupportedStatistics = supportedStatistics;
            ViewBag.SupportedCategories = supportedCategories;
            ViewBag.Clients = clients;
            return View();
        }

        public ActionResult Dohvati(string request) {
            var currentUser = authComponent.GetCurrentUser();
            var statistics = sc.GetJsonStatistics(request, currentUser);
            return Json(statistics, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Develop1() {
            return View();
        }

        public ActionResult Develop2() {
            return View();
        }

    }
}
