using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TCorp.JsonResponseModels {
    public class JsonPrice {
        public decimal? MonthlyPrice { get; set; }
        public decimal? OneTimePrice { get; set; }
    }
}