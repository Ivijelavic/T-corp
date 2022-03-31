using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TCorp.EntityFramework;
using TCorp.JsonIncomingModels.Statistika;
using TCorp.JsonResponseModels;
using TCorp.JsonResponseModels.Statistika;
using TCorp.Models;
using FunctorAlias = System.Func<TCorp.EntityFramework.Receipt, bool>;

namespace TCorp.Components {
    /// <summary>
    /// The class that holds the implementations of all the specific statistics available
    /// </summary>
    public class StatisticsEngine {
        private List<JsonSupportedStatistic> supportedStatistics = new List<JsonSupportedStatistic>();
        private Dictionary<int, Func<JsonStatisticsRequest, StatisticsBundle>> functionLookup = new Dictionary<int, Func<JsonStatisticsRequest, StatisticsBundle>>();
        private Dictionary<int, HashSet<int>> categoryParentsLookup = new Dictionary<int, HashSet<int>>();
        private Dictionary<int, List<int>> descendantsLookup = new Dictionary<int, List<int>>();
        private StatisticsXmlParser xmlParser = new StatisticsXmlParser();
        private StatisticsJsonParser jsonParser = new StatisticsJsonParser();
        public StatisticsEngine() {
            InitializeCategoryParentsLookup();
        }
        #region core

        private void InitializeCategoryParentsLookup() {
            using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                List<int?> parents = new List<int?> { 1, 2 };
                do {
                    var children = ctx.Categories
                        .Where(c => parents.Contains(c.ParentId))
                        .GroupBy(c => c.ParentId).ToList();
                    parents.Clear();
                    foreach (var childGroup in children) {
                        int parentId = (int)childGroup.Key;
                        if (categoryParentsLookup.ContainsKey(parentId) == false) {
                            categoryParentsLookup[parentId] = new HashSet<int>();
                        }
                        foreach (Category child in childGroup) {
                            categoryParentsLookup[parentId].Add(child.Id);
                            parents.Add(child.Id);
                        }
                    }
                } while (parents.Count > 0);
            }
        }

        public List<JsonSupportedStatistic> SupportedStatistics() {
            return supportedStatistics;
        }

        public void AddStatisticsOption(string name, Func<JsonStatisticsRequest, StatisticsBundle> function) {
            int index = supportedStatistics.Count + 1;
            JsonSupportedStatistic jss = new JsonSupportedStatistic();
            jss.Id = index;
            jss.Name = name;
            supportedStatistics.Add(jss);
            functionLookup.Add(index, function);
        }

        public JsonStatisticsResponse GetXmlStatistics(JsonStatisticsRequest request) {
            JsonStatisticsResponse response = new JsonStatisticsResponse();
            if (functionLookup.ContainsKey(request.StatisticOption) == false) throw new NotImplementedException("Tražena vrsta statistike nije implementirana");
            response.Status = JsonBasicResponse.OK;
            StatisticsBundle s = functionLookup[request.StatisticOption](request);
            response.Year = xmlParser.Parse(s.Year).InnerXml;
            response.Quarter = xmlParser.Parse(s.Quarter).InnerXml;
            response.Month = xmlParser.Parse(s.Month).InnerXml;
            return response;
        }
        public JsonStatisticsResponse GetJsonStatistics(JsonStatisticsRequest request) {
            JsonStatisticsResponse response = new JsonStatisticsResponse();
            if (functionLookup.ContainsKey(request.StatisticOption) == false) throw new NotImplementedException("Tražena vrsta statistike nije implementirana");
            response.Status = JsonBasicResponse.OK;
            StatisticsBundle s = functionLookup[request.StatisticOption](request);
            response.Year = jsonParser.Parse(s.Year);
            response.Quarter = jsonParser.Parse(s.Quarter);
            response.Month = jsonParser.Parse(s.Month);
            return response;
        }

        private List<int> GetDescendantNodes(int ancestorId) {
            if (descendantsLookup.ContainsKey(ancestorId)) {
                return descendantsLookup[ancestorId];
            }
            else {
                List<int> result = new List<int>();
                if (categoryParentsLookup.ContainsKey(ancestorId)) {
                    result.AddRange(categoryParentsLookup[ancestorId]);
                    List<int> descendants;
                    foreach (int descendantId in categoryParentsLookup[ancestorId]) {
                        descendants = GetDescendantNodes(descendantId);
                        result.AddRange(descendants);
                    }
                }
                if (result.Count > 0) {
                    descendantsLookup[ancestorId] = result;
                }
                return result;
            }
        }

        #endregion
        public StatisticsBundle BrojIzdanihPonuda(JsonStatisticsRequest request) {
            Func<IEnumerable<Receipt>, decimal> query = (r => r.Count());
            return CalculateStatistics(request, query);
        }

        public StatisticsBundle UkupniPrihod(JsonStatisticsRequest request) {
            Func<IEnumerable<Receipt>, decimal> query =
                (item => item.Sum(r => r.ReceiptBasket.Sum(rb =>
                    (rb.TotalPrice ?? 0) +
                    rb.ReceiptAdditionalOption.Sum(rao => rao.TotalPrice ?? 0))));
            return CalculateStatistics(request, query);
        }

        public StatisticsBundle PrihodOdOsnovneUsluge(JsonStatisticsRequest request) {
            Func<IEnumerable<Receipt>, decimal> query =
                (item => item.Sum(r => r.ReceiptBasket.Sum(rb => rb.TotalPrice ?? 0)));
            return CalculateStatistics(request, query);
        }

        public StatisticsBundle PrihodOdDodatnihOpcija(JsonStatisticsRequest request) {
            Func<IEnumerable<Receipt>, decimal> query =
                (item => item.Sum(r => r.ReceiptBasket.Sum(rb => rb.ReceiptAdditionalOption.Sum(rao => rao.TotalPrice ?? 0))));
            return CalculateStatistics(request, query);
        }

        /// <summary>
        /// The core method for calculating statistics
        /// </summary>
        /// <param name="request">The statistics request object</param>
        /// <param name="query">The query, passed as a functor</param>
        /// <returns>Returns the complete StatisticsBundle object</returns>
        private StatisticsBundle CalculateStatistics(JsonStatisticsRequest request, Func<IEnumerable<Receipt>, decimal> query) {
            StatisticsBundle result = new StatisticsBundle();
            using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                result.Year = new Statistics();
                result.Quarter = new Statistics();
                result.Month = new Statistics();
                Func<Receipt, bool> clientCondition;
                Func<Receipt, bool> categoryCondition;
                if (request.AllClients) {
                    clientCondition = (r => true);
                }
                else {
                    clientCondition = (r => r.client_id == request.ClientId);
                }
                if (request.AllCategories) {
                    categoryCondition = (r => true);
                }
                else {
                    List<int> descendantNodes = GetDescendantNodes(request.CategoryId);
                    categoryCondition = (r => r.ReceiptBasket.Any(rb => descendantNodes.Contains(rb.category_id)));
                }

                var resultsByYear = ctx.Receipts
                    .Where(r => r.user_id == request.UserId)
                    .Where(clientCondition)
                    .Where(categoryCondition)
                    .GroupBy(r => r.DateCreated.Year)
                    .Select(group => new { Year = group.Key, Value = query(group) })
                    .OrderBy(anon => anon.Year);
                foreach (var row in resultsByYear) {
                    StatisticsItem si = new StatisticsItem();
                    si.Name = String.Format("{0}.", row.Year);
                    si.Value = row.Value;
                    result.Year.Items.Add(si);
                }
                if (result.Year.Items.Count == 0) result.Year.Items.Add(new StatisticsItem { Name = DateTime.Now.Year.ToString(), Value = 0 });

                StatisticsItem[] monthItems = new StatisticsItem[12];
                StatisticsItem[] quarterItems = new StatisticsItem[4];
                for (int i = 0; i < 12; i++) {
                    monthItems[i] = new StatisticsItem();
                    monthItems[i].Name = String.Format("{0}. mj", i + 1);
                    monthItems[i].Value = 0;
                    result.Month.Items.Add(monthItems[i]);
                }
                for (int i = 0; i < 4; i++) {
                    quarterItems[i] = new StatisticsItem();
                    quarterItems[i].Name = String.Format("Q{0}", i + 1);
                    quarterItems[i].Value = 0;
                    result.Quarter.Items.Add(quarterItems[i]);
                }
                DateTime firstMonth = new DateTime(request.Year, 1, 1);
                DateTime lastMonth = new DateTime(request.Year + 1, 1, 1);
                var resultsByMonth = ctx.Receipts
                    .Where(r => r.user_id == request.UserId)
                    .Where(clientCondition)
                    .Where(categoryCondition)
                    .Where(r => r.DateCreated >= firstMonth && r.DateCreated < lastMonth)
                    .GroupBy(r => r.DateCreated.Month)
                    .Select(group => new { Month = group.Key, Value = query(group) })
                    .OrderBy(anon => anon.Month);
                foreach (var row in resultsByMonth) {
                    int monthIndex = row.Month - 1;
                    monthItems[monthIndex].Value = row.Value;
                    int quarterIndex = ((monthIndex - 1) / 3);
                    quarterItems[quarterIndex].Value += row.Value;
                }
            }
            return result;
        }
    }
}