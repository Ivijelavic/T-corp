using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TCorp.JsonIncomingModels.Racun {
    public class JsonAdditionalOption {
        /* Makni ove glupe stringove i stavi im njihov pravi tip
         * Ako to ne radi, onda znači da implicitno pretvaranje u decimal nije prošlo 
         * pa znači da i dalje moraš sam vodit računa o parsiranju */

        public string Name { get; set; }
        public string Value { get; set; }
        public string Price { get; set; }
        public string Index { get; set; }
        public string Discount { get; set; }
        public string Quantity { get; set; }
        public bool OneTimePayment { get; set; }
        public bool IncludedInBaseService { get; set; }
        public string Opis { get; set; }
        public string NazivPonude { get; set; }
        public bool ShowContractOfferName { get; set; }
    }
}