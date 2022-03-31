using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using TCorp.EntityFramework;
using TCorp.Logger;
using TCorp.Models;

namespace TCorp.Components {
    /// <summary>
    /// Handles all login related business logic. Buyer beware.
    /// </summary>
    public class AuthComponent {
        private static readonly BaseLogger logger = new DbLogger();
        /// <summary>
        /// Allowed roles to enter control (admin) panel. 
        /// </summary>
        public static readonly List<string> ADMIN_ALLOWED_ROLES = new List<string> {
            "Super Administrator"
        };

        public const int FAILED_LOGIN_LIMIT = 3;

        /// <summary>
        /// Returns the owner of the token if it is valid.
        /// Invalid tokens return null.
        /// </summary>
        /// <param name="token">The access token</param>
        /// <returns>Returns the owner of the token or null if invalid token</returns>
        public User GetTokenOwner(string token) {
            using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                User owner = ctx.Users
                    .AsNoTracking()
                    .Include(u => u.Role)
                    .Include("Role.PredefinedDiscount")
                    .SingleOrDefault(u =>
                        u.Session.AccessToken == token &&
                        (u.Session.ValidUntil == null || u.Session.ValidUntil > DateTime.Now));
                return owner;
            }
        }

        public UserViewModel.AuthState TryLogin(UserViewModel user) {
            return user.TryLogin();
        }

        /// <summary>
        /// Destroys the provided token
        /// </summary>
        /// <param name="token">The token being destroyed</param>
        /// <returns>Returns true on success, or false if the token was not valid</returns>
        public bool Logout(string token) {
            using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                Session session = ctx.Sessions.Include(s => s.Owner).SingleOrDefault(s => s.AccessToken == token);
                if (session == null) {
                    logger.Log(null, BaseLogger.WebAction.FailedLogout, token);
                    return false;
                }
                else {
                    logger.Log(session.Owner.Id, BaseLogger.WebAction.UserLogout, token);
                    ctx.Sessions.Remove(session);
                    ctx.SaveChanges();
                    return true;
                }
            }
        }
        public User GetCurrentUser() {
            User u = new User();
            try {
                int id = int.Parse(System.Web.HttpContext.Current.Session["Id"].ToString());
                using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                    u = ctx.Users.Include(user => user.Role).SingleOrDefault(user => user.Id == id);
                }
                //u.Username = System.Web.HttpContext.Current.Session["Username"].ToString();
                //u.Role = new Role();
                //u.Role.Id = int.Parse(System.Web.HttpContext.Current.Session["Role_id"].ToString());
                //u.Role.Name = System.Web.HttpContext.Current.Session["Role"].ToString();
            }
            catch (Exception) {
                return null;
            }
            return u;
        }

        public void SetSession(User user) {
            System.Web.HttpContext.Current.Session["Id"] = user.Id;
            System.Web.HttpContext.Current.Session["Username"] = user.Username;
            System.Web.HttpContext.Current.Session["Role"] = user.Role.Name;
            System.Web.HttpContext.Current.Session["Role_id"] = user.Role.Id;
        }

        /// <summary>
        /// Gets details about the provided username.
        /// Invalid usernames return null.
        /// </summary>
        /// <param name="username">The username being queried</param>
        /// <returns>Returns details about the username if it exists, otherwise returns null.</returns>
        public User GetUserDetails(string username) {
            using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                User user = ctx.Users.AsNoTracking().Include(u => u.Role).SingleOrDefault(u => u.Username == username);
                return user;
            }
        }

        public void IncreaseInvalidLoginCounter(UserViewModel userViewModel) {
            using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                User user = ctx.Users.SingleOrDefault(u => u.Username == userViewModel.Username);
                if (user != null) {
                    user.FailedLoginAttempts += 1;
                    ctx.SaveChanges();
                }
            }
        }

        public void Guard(ActionExecutingContext filterContext, List<int> allowedRoles) {
            string actionName = filterContext.ActionDescriptor.ActionName;
            string controllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            User currentUser = GetCurrentUser();
            if (currentUser == null || allowedRoles.Contains(currentUser.Role.Id) == false) {
                logger.Log(null, BaseLogger.WebAction.IntruderInCPanel, "Controller: " + controllerName + ", Action: " + actionName);
                filterContext.Result = new RedirectToRouteResult(
                    new System.Web.Routing.RouteValueDictionary {
                            { "Controller", "Izbornik" }, 
                            { "Action", "Login" }
                    });
            }
        }

        public void Guard(ActionExecutingContext filterContext) {
            string actionName = filterContext.ActionDescriptor.ActionName;
            string controllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            User currentUser = GetCurrentUser();
            if (currentUser == null) {
                filterContext.Result = new RedirectToRouteResult(
                    new System.Web.Routing.RouteValueDictionary {
                            {"Area", "ControlPanel"},
                            { "Controller", "Izbornik" }, 
                            { "Action", "Login" }
                    });
            }
        }
    }
}