using System.Web.Mvc;
using TCorp.JsonResponseModels;
using TCorp.Models;

namespace TCorp.Controllers {
    public class HomeController : CoreWebController {

        public ActionResult Index() {
            if (authComponent.GetCurrentUser() == null) {
                return RedirectToAction("Login", "Izbornik", new { Area = "ControlPanel" });
            }
            else {
                return View();
            }
        }

        public ActionResult Login() {
            return View();
        }

        [HttpPost]
        public ActionResult Login(UserViewModel userViewModel) {
            JsonLoginResponse json = new JsonLoginResponse();
            var loginResult = authComponent.TryLogin(userViewModel);
            switch (loginResult) {
                case UserViewModel.AuthState.OK:
                    json = userViewModel.CreateToken();
                    break;
                case UserViewModel.AuthState.Timeout:
                    json.Status = "Account time out"; //napiši vrijeme kad popusti timeout
                    break;
                case UserViewModel.AuthState.Ban:
                    json.Status = "Zabranjen Vam je pristup aplikaciji";
                    break;
                case UserViewModel.AuthState.WrongCredentials:
                    json.Status = "Neispravna kombinacija korisničkog imena i lozinke";
                    break;
                default:
                    json.Status = "Greška na poslužitelju";
                    break;
            }
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Logout(string token) {
            JsonBasicResponse json = new JsonBasicResponse();
            if (authComponent.Logout(token) == true) {
                json.Status = "ok";
                json.Data = "Uspio logout";
            }
            else {
                json.Status = "error";
                json.Data = "Invalid token";
            }
            return Json(json, JsonRequestBehavior.AllowGet);
        }
    }
}