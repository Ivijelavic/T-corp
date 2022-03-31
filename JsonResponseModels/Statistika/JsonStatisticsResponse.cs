using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TCorp.JsonResponseModels.Statistika {
    public class JsonStatisticsResponse {
        public string Status { get; set; }
        public string Year { get; set; }
        public string Quarter { get; set; }
        public string Month { get; set; }
        public string ErrorText { get; set; }
    }
}