using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TCorp.Models {
    public class StatisticsBundle {
        public Statistics Year { get; set; }
        public Statistics Quarter { get; set; }
        public Statistics Month { get; set; }
    }
}