using System.Web.Mvc;

namespace TCorp.Areas.MojProfil {
    public class MojProfilAreaRegistration : AreaRegistration {
        public override string AreaName {
            get {
                return "MojProfil";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) {
            context.MapRoute(
                "MojProfil_default",
                "MojProfil/{controller}/{action}/{id}",
                new { controller = "Izbornik", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}