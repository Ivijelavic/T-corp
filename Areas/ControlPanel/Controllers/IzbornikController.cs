using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Web;
using System.Web.Mvc;
using TCorp.Components;
using TCorp.Controllers;
using TCorp.Models;

namespace TCorp.Areas.ControlPanel.Controllers {
    public class IzbornikController : Controller {
        private AuthComponent login = new AuthComponent();

        public ActionResult Index() {
            return View();
        }

        public ActionResult Login() {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(UserViewModel userViewModel) {
            var loginResult = login.TryLogin(userViewModel);
            switch (loginResult) {
                case UserViewModel.AuthState.OK:
                    var user = login.GetUserDetails(userViewModel.Username);
                    if (user == null) {
                        throw new SecurityException("Invalid user logged in!");
                    }
                    login.SetSession(user);
                    return RedirectToAction("Index", "Home", new { Area = String.Empty });
                case UserViewModel.AuthState.Timeout:
                    TempData["Error"] = "Timeout";
                    break;
                case UserViewModel.AuthState.Ban:
                    TempData["Error"] = "Banned";
                    break;
                case UserViewModel.AuthState.WrongCredentials:
                    TempData["Error"] = "Wrong username or password";
                    break;
                default:
                    TempData["Error"] = "Server error";
                    break;
            }
            return View();
        }

        public ActionResult Logout() {
            Session.Clear();
            return View();
        }

        [NonAction]
        protected override void OnActionExecuting(ActionExecutingContext filterContext) {
            base.OnActionExecuting(filterContext);
            string actionName = filterContext.ActionDescriptor.ActionName;
            string controllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            if (actionName != "Login" && actionName != "Logout") {
                login.Guard(filterContext, SuperAdminController.ALLOWED_ROLES);
            }
        }
    }
}
