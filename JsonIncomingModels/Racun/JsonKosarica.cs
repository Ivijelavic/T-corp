using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TCorp.JsonIncomingModels.Racun {
    public class JsonKosarica {
        public List<JsonPaket> Paket { get; set; }

        public JsonKosarica() {
            Paket = new List<JsonPaket>();
        }
    }
}