using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TCorp.Models {
    public class Statistics {
        public int Width { get; set; }
        public List<StatisticsItem> Items { get; set; }

        public Statistics() {
            this.Width = 50;
            this.Items = new List<StatisticsItem>();
        }
    }
}