using System.Collections.Generic;
using TCorp.EntityFramework;

namespace TCorp.JsonResponseModels {
    public class JsonCategory {
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public string Info { get; set; }
        public double MaxDiscount { get; set; }
        public bool JednokratnoZaduzenje { get; set; }
        public bool BezOsnovneUsluge { get; set; }
        public int? InitialQuantity { get; set; }
        public decimal? InitialDiscount { get; set; }
        /* Dictionary keys must be strings in order to be serialized to json. 
         * INT DOESN'T WORK! */
        public Dictionary<string, JsonPrice> Prices { get; set; }
        public Dictionary<string, Dictionary<string, JsonPrice>> Discounts { get; set; }
        public List<JsonCategory> Children { get; set; }
        public bool HasStockInfo { get; set; }
        public int? QuantityInStock { get; set; }

        public JsonCategory() {
            Prices = new Dictionary<string, JsonPrice>();
            Discounts = new Dictionary<string, Dictionary<string, JsonPrice>>();
            Children = new List<JsonCategory>();
        }

        public void LoadPartial(Category category) {
            this.CategoryId = category.Id;
            this.Name = category.Name;
        }
    }
}