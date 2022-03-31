using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Web;
using TCorp.Components;
using TCorp.EntityFramework;
using TCorp.JsonIncomingModels.Racun;
using TCorp.ViewModels;
using System.Globalization;

namespace TCorp.ViewModels.Pdf {
    /// <summary>
    /// A ViewModel of the enitire pdf receipt. Handles descriptions and table generation.
    /// </summary>
    public class PdfViewModel {
        private List<ReceiptItemViewModel> receiptItems = new List<ReceiptItemViewModel>();
        private List<CategoryViewModel> descriptions = new List<CategoryViewModel>();
        private HashSet<int> descriptionsLookup = new HashSet<int>();

        private Dictionary<int, Category> categoryLookup;
        private Dictionary<int, Contract> contractLookup;

        public List<ReceiptItemViewModel> OneTimeItems {
            get {
                return receiptItems.Where(rivm => rivm.OneTime == true).ToList();
            }
        }

        public List<ReceiptItemViewModel> MonthlyItems {
            get {
                return receiptItems.Where(rivm => rivm.OneTime == false).ToList();
            }
        }

        public IEnumerable<CategoryViewModel> Descriptions {
            get {
                return descriptions;
            }
        }

        public User TManager { get; private set; }
        public DateTime Timestamp { get; set; }

        public decimal OneTimeTotalPrice {
            get {
                return this.OneTimeItems.Sum(ri => ri.TotalPrice);
            }
        }

        public decimal OneTimeTotalPriceWithPdv {
            get {
                return OneTimeTotalPrice * ConstantsComponent.PDV;
            }
        }

        public decimal MonthlyTotalPrice {
            get {
                return this.MonthlyItems.Sum(ri => ri.TotalPrice);
            }
        }

        public decimal MonthlyTotalPriceWithPdv {
            get {
                return MonthlyTotalPrice * ConstantsComponent.PDV;
            }
        }

        public PdfViewModel(int userId) {
            using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                User user = ctx.Users.AsNoTracking().Single(u => u.Id == userId); // try catch ako nema tog usera
                this.TManager = user;
            }
        }

        public void LoadFromRacun(JsonRacun racun) {
            using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                this.categoryLookup = ctx.Categories
                    .Include(c => c.Category_Price)
                    .Include(c => c.PredefinedDiscount_Category)
                    .AsNoTracking().ToDictionary(c => c.Id);
                this.contractLookup = ctx.Contract.AsNoTracking().ToDictionary(c => c.Id);
            }
            foreach (JsonPaket paket in racun.Kosarica.Paket) {
                AddItem(paket);
            }
        }

        /// <summary>
        /// Adds the JsonPaket item to the receipt. Description adding goes on here.
        /// </summary>
        /// <param name="paket">Stavka u računu</param>
        private void AddItem(JsonPaket paket) {
            int categoryId = int.Parse(paket.CategoryId);
            int contractId = paket.ContractId;
            bool isOneTimeItem = paket.JednokratnoZaduzenje;
            bool bezOsnovneUsluge = paket.BezOsnovneUsluge;
            Category originalCategory = categoryLookup[categoryId]; //try catch ako nema keya
            Contract contract = contractLookup[contractId]; //try catch ako nema keyaCont
            Category categoryPivot = originalCategory;
            if (bezOsnovneUsluge == false) {
                //pokupi opise
                List<CategoryViewModel> descriptionsBuffer = new List<CategoryViewModel>();
                while (categoryPivot.ParentId != null) {
                    if (descriptionsLookup.Contains(categoryPivot.Id) == false && String.Format("{0}", categoryPivot.Description).Trim() != String.Empty) {
                        descriptionsLookup.Add(categoryPivot.Id);
                        CategoryViewModel cvm = new CategoryViewModel();
                        cvm.Name = categoryPivot.Name;
                        cvm.Description = categoryPivot.Description;
                        descriptionsBuffer.Add(cvm);
                    }
                    Category parent = categoryLookup[(int)categoryPivot.ParentId];
                    originalCategory.Description = parent.Description + Environment.NewLine + originalCategory.Description;
                    categoryPivot.ParentCategory = parent;
                    categoryPivot = parent;
                }
                if (descriptionsBuffer.Count > 0) {
                    descriptionsBuffer.Reverse();
                    descriptions.AddRange(descriptionsBuffer);
                }
            }
            string name = String.Empty;
            if (originalCategory.ShowContractOfferName) {
                name = String.Format("{0} {1}", originalCategory.OfferName, contract.OfferName);
            }
            else {
                name = String.Format("{0}", originalCategory.OfferName);
            }
            int quantity = int.Parse(paket.Kolicina);
            int? discountId = paket.DiscountId;
            var priceBundle = originalCategory.Category_Price.Single(cp => cp.contract_id == contractId); //try catch
            var discountsBundle = originalCategory.PredefinedDiscount_Category.SingleOrDefault(cp => cp.predefinedDiscount_id == discountId);
            ReceiptItemViewModel monthlyUslugaViewModel = new ReceiptItemViewModel();
            ReceiptItemViewModel oneTimeUslugaViewModel = new ReceiptItemViewModel();
            if (bezOsnovneUsluge == false) {
                decimal price;
                if (discountsBundle == null) {
                    price = (decimal)priceBundle.MonthlyPrice;
                }
                else {
                    price = (decimal)discountsBundle.MonthlyPrice;
                }
                decimal discount = decimal.Parse(paket.Popust, NumberStyles.Float, CultureInfo.InvariantCulture);
                monthlyUslugaViewModel.Load(name, quantity, price, discount, false);
                receiptItems.Add(monthlyUslugaViewModel);
                if (isOneTimeItem) {
                    if (originalCategory.ShowContractOfferName) {
                        name = String.Format("{0} {1}", originalCategory.OfferNameOneTime, contract.OfferName);
                    }
                    else {
                        name = String.Format("{0}", originalCategory.OfferNameOneTime);
                    }
                    if (discountsBundle == null) {
                        price = (decimal)priceBundle.OneTimePrice;
                    }
                    else {
                        price = (decimal)discountsBundle.OneTimePrice;
                    }
                    discount = decimal.Parse(paket.JednokratnoZaduzenjePopust, NumberStyles.Float, CultureInfo.InvariantCulture);
                    oneTimeUslugaViewModel.Load(name, quantity, price, discount, true);
                    receiptItems.Add(oneTimeUslugaViewModel);
                }
            }
            foreach (JsonAdditionalOption dodatnaOpcija in paket.DodatneOpcije) {
                ReceiptItemViewModel dodatnaOpcijaViewModel = new ReceiptItemViewModel();
                dodatnaOpcijaViewModel.Load(dodatnaOpcija, contract);
                string decodedOpis = System.Web.HttpUtility.UrlDecode(dodatnaOpcija.Opis);
                originalCategory.Description = originalCategory.Description + Environment.NewLine + decodedOpis;
                if (dodatnaOpcija.IncludedInBaseService) {
                    monthlyUslugaViewModel.UnitPrice *= (1 - monthlyUslugaViewModel.Discount); // zruši cijenu osnovne usluge, da bi popust štimao
                    monthlyUslugaViewModel.Discount = 0;
                    monthlyUslugaViewModel.Name += String.Format(", {0}", dodatnaOpcija.NazivPonude);
                    monthlyUslugaViewModel.UnitPrice += dodatnaOpcijaViewModel.UnitPrice;
                }
                else {
                    receiptItems.Add(dodatnaOpcijaViewModel);
                }
            }
        }
    }
}