using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TCorp.EntityFramework;

namespace TCorp.JsonIncomingModels.Racun {
    public class JsonRacun {
        public string Id { get; set; } /* int */
        public JsonKosarica Kosarica { get; set; }
        public JsonGeo Geo { get; set; }
        public string ClientId { get; set; } /* int */
        public string Naziv { get; set; }
        public string Datum { get; set; } /* DateTime */
        public bool TemplateCorrupt { get; set; }

        public void Load(ReceiptTemplate template, User requestUser) {
            this.Id = template.Id.ToString();
            this.Naziv = template.Name;
            this.Kosarica = new JsonKosarica();
            this.Datum = template.DateCreated.ToString("dd.MM.yyyy");
            foreach (ReceiptTemplateBasket rtb in template.ReceiptTemplateBasket) {
                JsonPaket jp = new JsonPaket();
                jp.CategoryId = rtb.category_id.ToString();
                jp.Cijena = rtb.Price.ToString();
                jp.Kolicina = rtb.Quantity.ToString();
                jp.Popust = rtb.Discount.ToString();
                var maxDiscount = rtb.Category.Category_Role_MaxDiscount.SingleOrDefault(c => c.role_id == requestUser.role_id);
                jp.MaxDiscount = (maxDiscount == null) ? "0" : maxDiscount.MaxDiscount.ToString();
                jp.Naziv = rtb.Category.Name;
                jp.NazivVrsteRada = rtb.VrstaRada;
                jp.ContractId = rtb.contract_id;
                jp.DiscountId = rtb.discount_id ?? -1;
                this.Kosarica.Paket.Add(jp);

                foreach (ReceiptDefaultOption rdo in rtb.ReceiptDefaultOption) {
                    JsonAdditionalOption jao = new JsonAdditionalOption();
                    jao.Name = rdo.Name;
                    jao.Value = rdo.Value;
                    jao.Price = rdo.Price.ToString();
                    jao.Index = rdo.Index.ToString();
                    jp.DodatneOpcije.Add(jao);
                }
            }
        }

        public void LoadHeader(ReceiptTemplate template) {
            this.Id = template.Id.ToString();
            this.Naziv = template.Name;
            this.Datum = template.DateCreated.ToString("dd.MM.yyyy");
        }
    }
}