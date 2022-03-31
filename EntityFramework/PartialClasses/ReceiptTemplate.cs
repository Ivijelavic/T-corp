using System;
using System.Globalization;
using System.Web;
using TCorp.JsonIncomingModels.Racun;

namespace TCorp.EntityFramework {
    public partial class ReceiptTemplate {
        public void Load(JsonRacun racun, User requestUser) {
            if (requestUser == null) {
                throw new ArgumentNullException("Invalid (null) requestUser in public void ReceiptTemplate.Load(User requestUser, JsonRacun json)");
            }
            int categoryId, quantity;
            decimal price, discount, oneTimeDiscount;
            this.IP = HttpContext.Current.Request.UserHostAddress;
            this.DateCreated = DateTime.Now;
            this.user_id = requestUser.Id;
            this.Name = racun.Naziv;
            foreach (JsonPaket jp in racun.Kosarica.Paket) {
                ReceiptTemplateBasket rtb = new ReceiptTemplateBasket();
                if (int.TryParse(jp.CategoryId, out categoryId) == false) {
                    throw new ArgumentException("CategoryId is not an integer");
                }
                rtb.category_id = categoryId;
                if (decimal.TryParse(jp.Cijena, NumberStyles.Float, CultureInfo.InvariantCulture, out price) == false) {
                    throw new ArgumentException("Cijena is not an integer");
                }
                rtb.Price = price;
                if (int.TryParse(jp.Kolicina, out quantity) == false) {
                    throw new ArgumentException("Kolicina is not an integer");
                }
                rtb.Quantity = quantity;
                if (decimal.TryParse(jp.Popust, NumberStyles.Float, CultureInfo.InvariantCulture, out discount) == false) {
                    throw new ArgumentException("Popust is not a real number");
                }
                if (decimal.TryParse(jp.JednokratnoZaduzenjePopust, NumberStyles.Float, CultureInfo.InvariantCulture, out oneTimeDiscount) == false) {
                    throw new ArgumentException("Popust is not a real number");
                }
                rtb.Discount = discount;
                rtb.ReceiptTemplate = this;
                rtb.VrstaRada = jp.NazivVrsteRada;
                rtb.OneTimeDiscount = oneTimeDiscount;
                rtb.JednokratnoZaduzenje = jp.JednokratnoZaduzenje;
                rtb.BezOsnovneUsluge = jp.BezOsnovneUsluge;
                rtb.contract_id = jp.ContractId;
                rtb.discount_id = jp.DiscountId;
                foreach (JsonAdditionalOption jao in jp.DodatneOpcije) {
                    ReceiptDefaultOption rdo = new ReceiptDefaultOption();
                    rdo.Name = jao.Name;
                    rdo.Price = decimal.Parse(jao.Price, NumberStyles.Float, CultureInfo.InvariantCulture);
                    rdo.Value = jao.Value;
                    try {
                        rdo.Index = int.Parse(jao.Index);
                    }
                    catch (Exception) {
                        rdo.Index = null;
                    }
                    rdo.ReceiptTemplateBasket = rtb;
                    rtb.ReceiptDefaultOption.Add(rdo);
                }
                this.ReceiptTemplateBasket.Add(rtb);
            }
        }
    }
}