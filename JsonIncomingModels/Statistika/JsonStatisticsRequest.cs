using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TCorp.JsonIncomingModels.Statistika {
    public class JsonStatisticsRequest {
        public int UserId { get; set; }
        public int CategoryId { get; set; }
        public bool AllCategories { get; set; }
        public int ClientId { get; set; }
        public bool AllClients { get; set; }
        public int StatisticOption { get; set; }
        public int Year { get; set; }
    }
}