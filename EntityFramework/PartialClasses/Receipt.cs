using System;
using System.Globalization;
using System.Linq;
using System.Web;
using TCorp.Components;
using TCorp.JsonIncomingModels.Racun;

namespace TCorp.EntityFramework {
    public partial class Receipt {
        public void Load(JsonRacun racun, string pdfName, User requestUser) {
            if (requestUser == null) {
                throw new ArgumentNullException("Invalid (null) requestUser in public void Receipt.Load(User requestUser, JsonRacun json)");
            }
            int clientId, categoryId, quantity;
            decimal price, discount, oneTimeDiscount;
            if (int.TryParse(racun.ClientId, out clientId) == false) {
                throw new ArgumentException("ClientId is not an integer");
            }
            /* validacija dal uopce ovaj smije posluzit tog klijenta */
            /* validacija dal smije izdat toliki popust */
            this.client_id = clientId;
            this.Geo = new Geo();
            this.Geo.Latitude = racun.Geo.Latitude;
            this.Geo.Longitude = racun.Geo.Longitude;
            this.IP = HttpContext.Current.Request.UserHostAddress;
            this.DateCreated = DateTime.Now;
            this.user_id = requestUser.Id;
            this.PdfName = pdfName;
            foreach (JsonPaket jp in racun.Kosarica.Paket) {
                ReceiptBasket rb = new ReceiptBasket();
                if (int.TryParse(jp.CategoryId, out categoryId) == false) {
                    throw new ArgumentException("CategoryId is not an integer");
                }
                rb.category_id = categoryId;
                if (decimal.TryParse(jp.Cijena, NumberStyles.Float, CultureInfo.InvariantCulture, out price) == false) {
                    throw new ArgumentException("Cijena is not an integer");
                }
                rb.Price = price;
                if (int.TryParse(jp.Kolicina, out quantity) == false) {
                    throw new ArgumentException("Kolicina is not an integer");
                }
                rb.Quantity = quantity;
                if (decimal.TryParse(jp.Popust, NumberStyles.Float, CultureInfo.InvariantCulture, out discount) == false) {
                    throw new ArgumentException("Popust is not a real number");
                }
                if (decimal.TryParse(jp.JednokratnoZaduzenjePopust, NumberStyles.Float, CultureInfo.InvariantCulture, out oneTimeDiscount) == false) {
                    throw new ArgumentException("Popust is not a real number");
                }
                rb.Discount = discount;
                rb.Receipt = this;
                rb.VrstaRada = jp.NazivVrsteRada;
                rb.OneTimeDiscount = oneTimeDiscount;
                rb.JednokratnoZaduzenje = jp.JednokratnoZaduzenje;
                rb.BezOsnovneUsluge = jp.BezOsnovneUsluge;
                rb.contract_id = jp.ContractId;
                rb.discount_id = jp.DiscountId;
                foreach (JsonAdditionalOption jao in jp.DodatneOpcije) {
                    ReceiptAdditionalOption rao = new ReceiptAdditionalOption();
                    rao.Name = jao.Name;
                    rao.Price = decimal.Parse(jao.Price, NumberStyles.Float, CultureInfo.InvariantCulture);
                    rao.Value = jao.Value;
                    if (jao.Discount == null) {
                        rao.Discount = null;
                    }
                    else {
                        rao.Discount = decimal.Parse(jao.Discount, NumberStyles.Float, CultureInfo.InvariantCulture) / 100;
                    }
                    if (jao.Quantity == null) {
                        rao.Quantity = null;
                    }
                    else {
                        rao.Quantity = int.Parse(jao.Quantity);
                    }
                    if (jao.Index == null) {
                        rao.Index = null;
                    }
                    else {
                        rao.Index = int.Parse(jao.Index);
                    }
                    rao.JednokratnoZaduzenje = jao.OneTimePayment;
                    rao.ReceiptBasket = rb;
                    rb.ReceiptAdditionalOption.Add(rao);
                }
                this.ReceiptBasket.Add(rb);
            }
        }
    }
}