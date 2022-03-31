using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TCorp.Controllers {
    /// <summary>
    /// All methods in this controller require the super administrator role to access. To get finer access controler, 
    /// use CoreWebController and put guards at the beggining of each action.
    /// </summary>
    public class SuperAdminController : CoreWebController {
        public static readonly List<int> ALLOWED_ROLES = new List<int>() {
            1 /* Super Administrator */
        };
        [NonAction]
        protected override void OnActionExecuting(ActionExecutingContext filterContext) {
            base.OnActionExecuting(filterContext);
            authComponent.Guard(filterContext, ALLOWED_ROLES);
        }
    }
}
