using System;
using System.Web;
using TCorp.EntityFramework;

namespace TCorp.Logger {
    /// <summary>
    /// Saves the logs in the database
    /// </summary>
    public class DbLogger : BaseLogger {
        /// <summary>
        /// Performs the logging operation
        /// </summary>
        /// <param name="userId">The user performing the action</param>
        /// <param name="action">The actual action being logged</param>
        /// <param name="desc">A short description of the action</param>
        public override void Log(int? userId, WebAction action, string desc) {
            using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                string ip = HttpContext.Current.Request.UserHostAddress;
                DateTime now = DateTime.Now;
                WebLog wl = new WebLog();
                wl.IP = ip;
                wl.LogDate = now;
                wl.user_id = userId;
                wl.action_id = (int)action;
                wl.Description = desc;
                ctx.WebLogs.Add(wl);
                ctx.SaveChanges();
            }
        }
    }
}