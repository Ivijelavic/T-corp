using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TCorp.JsonResponseModels;

namespace TCorp.JsonIncomingModels.Racun {
    public class JsonPaket {
        public string CategoryId { get; set; } /* int */
        public string Naziv { get; set; } /* string */
        public string Kolicina { get; set; } /* int */
        public string Cijena { get; set; }
        public bool JednokratnoZaduzenje { get; set; }
        public bool BezOsnovneUsluge { get; set; }
        public List<JsonPrice> Prices { get; set; }
        public string Popust { get; set; } /* decimal */
        public string JednokratnoZaduzenjePopust { get; set; } /* decimal */
        public string MaxDiscount { get; set; } /* decimal */
        public string NazivVrsteRada { get; set; }
        public int ContractId { get; set; } /* int */
        public bool DodatneOpcijeCorrupt { get; set; }
        public List<JsonAdditionalOption> DodatneOpcije { get; set; }
        public int? DiscountId { get; set; }

        public JsonPaket() {
            DodatneOpcije = new List<JsonAdditionalOption>();
            Prices = new List<JsonPrice>();
        }
    }
}