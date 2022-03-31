using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TCorp.JsonResponseModels {
    public class JsonDiscount {
        public string DiscountId { get; set; }
        public string Name { get; set; }
        public int? DurationInMonths { get; set; }
    }
}