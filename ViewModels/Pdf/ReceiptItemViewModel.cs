using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using TCorp.Components;
using TCorp.EntityFramework;
using TCorp.JsonIncomingModels.Racun;

namespace TCorp.ViewModels.Pdf {
    /// <summary>
    /// The model for an indivitual entity in the pdf receipt.
    /// Svaka redak u tablici u pdf-u je jedan ReceiptItemViewModel.
    /// </summary>
    public class ReceiptItemViewModel {
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Discount { get; set; }
        public bool OneTime { get; set; }

        public decimal UnitPriceWithPdv {
            get {
                return UnitPrice * ConstantsComponent.PDV;
            }
        }

        public decimal TotalPrice {
            get {
                return Quantity * UnitPrice * (1 - Discount);
            }
        }

        public decimal TotalPriceWithPdv {
            get {
                return TotalPrice * ConstantsComponent.PDV;
            }
        }

        public void Load(string name, int quantity, decimal price, decimal discount, bool oneTime) {
            this.Name = name;
            this.Quantity = quantity;
            this.UnitPrice = price;
            this.Discount = discount;
            this.OneTime = oneTime;
        }

        public void Load(JsonAdditionalOption dodatnaOpcija, Contract contract) {
            //jao koji sataraš
            this.Name = (dodatnaOpcija.NazivPonude == null || dodatnaOpcija.NazivPonude == String.Empty) ? dodatnaOpcija.Value : dodatnaOpcija.NazivPonude;
            if (dodatnaOpcija.ShowContractOfferName) {
                this.Name = String.Format("{0} {1}", this.Name, contract.OfferName);
            }
            this.Quantity = int.Parse(dodatnaOpcija.Quantity);
            this.UnitPrice = decimal.Parse(dodatnaOpcija.Price, NumberStyles.Float, CultureInfo.InvariantCulture);
            this.Discount = decimal.Parse(dodatnaOpcija.Discount, NumberStyles.Float, CultureInfo.InvariantCulture) / 100;
            this.OneTime = dodatnaOpcija.OneTimePayment;
        }
    }
}