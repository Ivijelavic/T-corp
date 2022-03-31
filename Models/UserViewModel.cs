using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Security;
using TCorp.Components;
using TCorp.EntityFramework;
using TCorp.JsonResponseModels;
using TCorp.Logger;

namespace TCorp.Models {
    public class UserViewModel {

        public enum AuthState {
            OK,
            Timeout,
            Ban,
            WrongCredentials
        }

        [Required]
        [Display(Name = "Korisničko ime")]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Lozinka")]
        public string Password { get; set; }

        private CryptoComponent crypto = new CryptoComponent();
        private static readonly BaseLogger logger = new DbLogger();

        public AuthState TryLogin() {
            using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                var user = ctx.Users.SingleOrDefault(u => u.Username == Username);
                if (user == null) {
                    logger.Log(null, BaseLogger.WebAction.WrongUsername, Username);
                    return AuthState.WrongCredentials;
                }
                var lastLoginRequest = user.LastLoginRequest;
                if (lastLoginRequest == null || (DateTime.Now - lastLoginRequest).Value.TotalMinutes >= 30) {
                    user.FailedLoginAttempts = 0;
                }
                if (user.IsBanned == true) {
                    logger.Log(user.Id, BaseLogger.WebAction.BannedLogin, null);
                    return AuthState.Ban;
                }
                if (user.FailedLoginLimitReached()) {
                    logger.Log(user.Id, BaseLogger.WebAction.TimedOutLogin, null);
                    return AuthState.Timeout;
                }

                user.LastLoginRequest = DateTime.Now;
                string hashedPassword = crypto.Hash(this.Password, user.Salt);
                if (user.Password == hashedPassword) {
                    user.FailedLoginAttempts = 0;
                    ctx.SaveChanges();
                    logger.Log(user.Id, BaseLogger.WebAction.UserLogin, null);
                    return AuthState.OK;
                }
                else {
                    user.FailedLoginAttempts += 1;
                    ctx.SaveChanges();
                    logger.Log(user.Id, BaseLogger.WebAction.WrongPassword, null);
                    return AuthState.WrongCredentials;
                }
            }
        }

        /// <summary>
        /// Creates a secure token for the logged in user and returns his details
        /// in JSON
        /// </summary>
        /// <returns>User details in JSON</returns>
        public JsonLoginResponse CreateToken() {
            JsonLoginResponse response = new JsonLoginResponse();
            using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                User user = ctx.Users.Include(u => u.Role).Include(u => u.Session).SingleOrDefault(u => u.Username == this.Username);
                if (user == null) {
                    if (user == null) {
                        logger.Log(null, BaseLogger.WebAction.SecurityException, "Attempted to create a token for non-existing username");
                        throw new SecurityException("Attempted to create token for non-existing username");
                    }
                }
                string hashedPassword = crypto.Hash(this.Password, user.Salt);
                if (user.Password != hashedPassword) {
                    logger.Log(null, BaseLogger.WebAction.SecurityException, "Attempted to create a token for a user with an invalid password");
                    throw new SecurityException("Attempted to create a token for a user with an invalid password");
                }
                Session s = user.Session;
                if (s == null) {
                    s = new Session();
                    s.Load(user);
                    user.Session = s;
                    ctx.Sessions.Add(s);
                }
                else {
                    s.Load(user);
                }
                ctx.SaveChanges();
                logger.Log(user.Id, BaseLogger.WebAction.TokenCreated, s.AccessToken);
                response.Load(user, s);
                return response;
            }
        }
    }
}