using System;
using System.Collections.Generic;
using System.Linq;
using TCorp.JsonResponseModels;

namespace TCorp.EntityFramework {
    public partial class Category {
        public bool QuantityInStockTracked {
            get {
                return (this.Category_InStock != null);
            }
        } 
        public bool QuantityInStockKnown {
            get {
                return QuantityInStockTracked && (this.Category_InStock.Quantity != null);
            }
        }
        public int? QuantityInStock {
            get {
                return (this.Category_InStock == null) ? null : this.Category_InStock.Quantity;
            }
        }

        public void Load(string name, int? parentId, bool isHidden, string desc, string info) {
            this.Name = name;
            this.ParentId = parentId;
            this.IsHidden = isHidden;
            this.Description = desc;
            this.Info = info;
        }

        public Dictionary<string, JsonPrice> PricesToJson() {
            Dictionary<string, JsonPrice> result = new Dictionary<string, JsonPrice>();
            foreach (Category_Price cp in this.Category_Price) {
                JsonPrice jp = new JsonPrice();
                jp.MonthlyPrice = cp.MonthlyPrice;
                jp.OneTimePrice = cp.OneTimePrice;
                result.Add(cp.Contract.Id.ToString(), jp);
            }
            return result;
        }

        public Dictionary<string, Dictionary<string, JsonPrice>> DiscountsToJson(User requestUser) {
            Dictionary<string, Dictionary<string, JsonPrice>> result = new Dictionary<string, Dictionary<string, JsonPrice>>();
            var allDiscountsForCategory = this.PredefinedDiscount_Category;
            var availableDiscounts = requestUser.Role.PredefinedDiscount.Select(pd => pd.Id);
            var visibleDiscounts = allDiscountsForCategory.Where(pd => availableDiscounts.Contains(pd.predefinedDiscount_id));
            var groupedDiscouts = visibleDiscounts.GroupBy(vd => vd.PredefinedDiscount.contract_id);
            foreach (var group in groupedDiscouts) {
                string contractId = group.Key.ToString();
                Dictionary<string, JsonPrice> discountsForContract = new Dictionary<string, JsonPrice>();
                foreach (PredefinedDiscount_Category discount in group) {
                    string discountId = discount.predefinedDiscount_id.ToString();
                    JsonPrice jp = new JsonPrice();
                    jp.MonthlyPrice = discount.MonthlyPrice;
                    jp.OneTimePrice = discount.OneTimePrice;
                    discountsForContract.Add(discountId, jp);
                }
                result.Add(contractId, discountsForContract);
            }
            return result;
        }

        public void Update(string name, List<int> Contracts, List<string> MonthlyPrices, List<string> OneTimePrices, bool showContractOfferName, bool isHidden, bool isDeleted, string desc, string info, int? initialQuantity, decimal? initialDiscount, string offerName, string offerNameOneTime) {
            this.Name = name;
            this.IsHidden = isHidden;
            this.IsDeleted = isDeleted;
            this.ShowContractOfferName = showContractOfferName;
            this.Description = desc;
            this.Info = info;
            this.InitialQuantity = initialQuantity;
            this.InitialDiscount = initialDiscount;
            this.OfferName = offerName;
            this.OfferNameOneTime = offerNameOneTime;
            for (int i = 0; i < Contracts.Count; i++) {
                Category_Price cp = this.Category_Price.SingleOrDefault(p => p.contract_id == Contracts[i]);
                if (MonthlyPrices[i] == String.Empty && OneTimePrices[i] == String.Empty) {
                    /* Kategorija nije dostupna pod tim ugovorom. Potrebno ju je ukloniti ako postoji od prije */
                    if (cp != null) {
                        this.Category_Price.Remove(cp);
                    }
                }
                else {
                    /* Kategorija je dostupna - Potrebno ju je dodati ili osvježiti cijene */
                    if (cp == null) {
                        /* Stvori novi entry u bazi */
                        cp = new Category_Price();
                        cp.category_id = this.Id;
                        cp.contract_id = Contracts[i];
                        if (MonthlyPrices[i] == String.Empty) {
                            cp.MonthlyPrice = null;
                        }
                        else {
                            cp.MonthlyPrice = decimal.Parse(MonthlyPrices[i]);
                        }
                        if (OneTimePrices[i] == String.Empty) {
                            cp.OneTimePrice = null;
                        }
                        else {
                            cp.OneTimePrice = decimal.Parse(OneTimePrices[i]);
                        }
                        this.Category_Price.Add(cp);
                    }
                    else {
                        /* samo osvježi postojeći */
                        if (MonthlyPrices[i] == String.Empty) {
                            cp.MonthlyPrice = null;
                        }
                        else {
                            cp.MonthlyPrice = decimal.Parse(MonthlyPrices[i]);
                        }
                        if (OneTimePrices[i] == String.Empty) {
                            cp.OneTimePrice = null;
                        }
                        else {
                            cp.OneTimePrice = decimal.Parse(OneTimePrices[i]);
                        }
                    }
                }
            }
        }

        public bool IsVisibleFor(User user) {
            return (
                this.IsHidden == false &&
                this.IsDeleted == false &&
                this.Category_Role_MaxDiscount.Any(crm => crm.role_id == user.role_id));
        }
    }
}
