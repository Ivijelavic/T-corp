using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TCorp.JsonResponseModels.Statistika {
    public class JsonStatisticsContract {
        public string Status { get; set; }
        public List<JsonCategory> InitialCategories { get; set; }
        public List<JsonSupportedStatistic> SupportedStatistics { get; set; }
        public List<JsonClient> Clients { get; set; }
        public JsonStatisticsContract() {
            InitialCategories = new List<JsonCategory>();
            SupportedStatistics = new List<JsonSupportedStatistic>();
            Clients = new List<JsonClient>();
        }
    }
}