using System;
namespace TCorp.EntityFramework {
    public partial class Client {
        public string DisplayName {
            get {
                return String.Format("{0} {1}", this.Ime, this.Prezime);
            }
        }
    }
}