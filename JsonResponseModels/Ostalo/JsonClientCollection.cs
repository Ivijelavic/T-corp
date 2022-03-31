using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TCorp.JsonResponseModels.Ostalo {
    public class JsonClientCollection {
        public string Error { get; set; }
        public List<JsonClient> PersonalClients { get; set; }
        public List<JsonClient> SharedClients { get; set; }
        public JsonClientCollection() {
            PersonalClients = new List<JsonClient>();
            SharedClients = new List<JsonClient>();
            Error = null;
        }
    }
}