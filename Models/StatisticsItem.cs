using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TCorp.Models {
    public class StatisticsItem {
        public string Name { get; set; }
        public decimal Value { get; set; }
        public string Color { get; set; }

        public StatisticsItem() {
            Color = "0xe20074";
            Value = 0;
        }
    }
}