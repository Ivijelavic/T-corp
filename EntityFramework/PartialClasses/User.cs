using System;
using TCorp.Areas.ControlPanel.Models;
using TCorp.Components;

namespace TCorp.EntityFramework {
    public partial class User {
        private static readonly CryptoComponent crypto = new CryptoComponent();

        public string DisplayName {
            get {
                return String.Format("{0} {1}", this.Firstname, this.Lastname);
            }
        }

        public bool FailedLoginLimitReached() {
            return (FailedLoginAttempts >= AuthComponent.FAILED_LOGIN_LIMIT);
        }

        public void Load(NewUserViewModel user) {
            this.Salt = crypto.GenerateSalt();
            this.Username = user.Username;
            this.Password = crypto.Hash(user.Password, this.Salt);
            this.Email = user.Email;
            this.Firstname = user.Firstname;
            this.Lastname = user.Lastname;
            this.IsBanned = user.IsBanned;
            this.role_id = user.RoleId;
            this.organization_id = user.OrgId;
            this.BccField = user.BccField;
            if (user.NoSuperior) {
                this.superiorUser_id = null;
            }
            else {
                this.superiorUser_id = user.SuperiorId;
            }
        }

        public void Update(NewUserViewModel user) {
            this.Username = user.Username;
            this.Email = user.Email;
            this.Firstname = user.Firstname;
            this.Lastname = user.Lastname;
            this.IsBanned = user.IsBanned;
            if (String.IsNullOrEmpty(user.Password) == false) {
                this.Password = crypto.Hash(user.Password, this.Salt);
            }
            this.role_id = user.RoleId;
            this.organization_id = user.OrgId;
            this.BccField = user.BccField;
            if (user.NoSuperior) {
                this.superiorUser_id = null;
            }
            else {
                this.superiorUser_id = user.SuperiorId;
            }
        }
    }
}
