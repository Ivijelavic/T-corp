using System.Web.Mvc;

namespace TCorp.Areas.ControlPanel {
    public class ControlPanelAreaRegistration : AreaRegistration {
        public override string AreaName {
            get {
                return "ControlPanel";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) {
            context.MapRoute(
                "ControlPanel_default",
                "ControlPanel/{controller}/{action}/{id}",
                new { controller = "Izbornik", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
