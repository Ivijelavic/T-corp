using TCorp.Components;
using TCorp.EntityFramework;
namespace TCorp.JsonResponseModels {
    public class JsonLoginResponse {
        private const string NO_SUPERIOR = "Nema nadređenog";

        public string Status { get; set; }
        public string Token { get; set; }
        public string Ime { get; set; }
        public string Prezime { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Cellphone { get; set; }
        public string Fax { get; set; }
        public string Address { get; set; }
        public string KPNumber { get; set; }
        public string OIB { get; set; }
        public string Superior { get; set; }
        public string Role { get; set; }
        public bool SpecialAccount { get; set; }

        public JsonLoginResponse() {
            Status = "undefined";
        }

        public void Load(User user, Session s) {
            this.Status = JsonBasicResponse.OK;
            this.Token = s.AccessToken;
            this.Ime = user.Firstname;
            this.Prezime = user.Lastname;
            this.Address = user.Address;
            this.Cellphone = user.Cellphone;
            this.Email = user.Email;
            this.Fax = user.Fax;
            this.KPNumber = user.KPNumber;
            this.OIB = user.OIB;
            this.Phone = user.Phone;
            this.Role = user.Role.Name;
            this.Superior = (user.Superior == null) ? NO_SUPERIOR : user.Superior.DisplayName;
            this.SpecialAccount = user.Role.ManagerPrivilages;
        }
    }
}