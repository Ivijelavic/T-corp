using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TCorp.Controllers {
    /// <summary>
    /// Controller that required the visitor to be logged in
    /// </summary>
    public class ValidUserController : CoreWebController {
        [NonAction]
        protected override void OnActionExecuting(ActionExecutingContext filterContext) {
            base.OnActionExecuting(filterContext);
            authComponent.Guard(filterContext);
        }
    }
}