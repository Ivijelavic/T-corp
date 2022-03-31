using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TCorp.Controllers;

namespace TCorp.Areas.MojProfil.Controllers {
    public class IzbornikController : ValidUserController {
        //
        // GET: /MojProfil/Izbornik/

        public ActionResult Index() {
            return View();
        }

        public ActionResult Develop1() {
            return View();
        }

        public ActionResult Develop2() {
            return View();
        }

    }
}
