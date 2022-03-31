using System.Collections.Generic;
using System.Web.Mvc;
using TCorp.Components;
using TCorp.Logger;

namespace TCorp.Controllers {
    /// <summary>
    /// Core web controller that provides a DbLogger and AuthComponent to all inheriting controllers
    /// </summary>
    public abstract class CoreWebController : Controller {
        protected static readonly BaseLogger logger = new DbLogger();
        protected static readonly AuthComponent authComponent = new AuthComponent();
    }
}