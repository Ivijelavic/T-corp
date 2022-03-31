using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TCorp.Components;
using TCorp.EntityFramework;

namespace TCorp.Areas.ControlPanel.Models {
    public class NewUserViewModel : User {
        public string Password2 { get; set; }
        public int RoleId { get; set; }
        public int OrgId { get; set; }
        public int SuperiorId { get; set; }
        public bool NoSuperior { get; set; }
        public bool ValidateUpdate(System.Web.Mvc.TempDataDictionary TempData) {
            if (Password != Password2) {
                TempData["PasswordError"] = "Passwords do not match";
                return false;
            }
            return true;
        }

        public bool ValidateCreate(System.Web.Mvc.TempDataDictionary TempData) {
            if (String.IsNullOrEmpty(Password)) {
                TempData["PasswordError"] = "Please enter a password";
                return false;
            }
            if (Password != Password2) {
                TempData["PasswordError"] = "Passwords do not match";
                return false;
            }
            return true;
        }
    }
}