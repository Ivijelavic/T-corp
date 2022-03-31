using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TCorp.EntityFramework;

namespace TCorp.JsonResponseModels {
    public class JsonClient {
        public string Id { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string ContactNumber { get; set; }
        public string OIB { get; set; }
        public string Company { get; set; }

        public string DisplayName {
            get {
                return Firstname + " " + Lastname;
            }
        }

        public void Load(Client client) {
            this.Id = client.Id.ToString();
            this.Firstname = client.Ime;
            this.Lastname = client.Prezime;
            this.Email = client.Email;
            this.Address = client.Adresa;
            this.Company = client.Tvrtka;
            this.ContactNumber = client.KontaktBroj;
            this.OIB = client.OIB;
        }
    }
}