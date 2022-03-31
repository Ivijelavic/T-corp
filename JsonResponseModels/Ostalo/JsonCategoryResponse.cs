using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TCorp.Components;

namespace TCorp.JsonResponseModels {
    public class JsonCategoryResponse {
        public List<JsonCategory> CategoryTree { get; set; }
        public List<JsonCategory> CategoryList { get; set; }
        public decimal PDV {
            get {
                return ConstantsComponent.PDV;
            }
        }
        public List<JsonContract> Contracts { get; set; }
        public List<JsonDiscount> Discounts { get; set; }
        public List<string> Clusters { get; set; }
    }
}