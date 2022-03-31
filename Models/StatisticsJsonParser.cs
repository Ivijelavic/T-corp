using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TCorp.Models {
    public class StatisticsJsonParser {
        public string Parse(Statistics statistics) {
            string data = String.Empty;
            int count = statistics.Items.Count;
            if (count > 0) {
                for (int i = 0; i < count - 1; i++) {
                    StatisticsItem item = statistics.Items[i];
                    data += String.Format(@"[""{0}"", {1}],", item.Name, item.Value.ToString().Replace(",","."));
                }
                StatisticsItem lastItem = statistics.Items.Last();
                data += String.Format(@"[""{0}"", {1}]", lastItem.Name, lastItem.Value.ToString().Replace(",", "."));
            }
            string result = String.Format("[{0}]", data);
            return result;
        }
    }
}