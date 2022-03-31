using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using TCorp.EntityFramework;
using TCorp.JsonIncomingModels.Statistika;
using TCorp.JsonResponseModels;
using TCorp.JsonResponseModels.Statistika;
using TCorp.Models;

namespace TCorp.Components {
    /// <summary>
    /// Handles all things statistics related. Singleton
    /// </summary>
    public class StatisticsComponent {
        private static StatisticsComponent sc;
        public static StatisticsComponent Instance {
            get {
                if (sc == null) {
                    sc = new StatisticsComponent();
                }
                return sc;
            }
        }
        private static StatisticsEngine se;

        /// <summary>
        /// Initialize all the available statistics options in the constructor
        /// </summary>
        private StatisticsComponent() {
            se = new StatisticsEngine();
            se.AddStatisticsOption("Broj izdanih ponuda", se.BrojIzdanihPonuda);
            se.AddStatisticsOption("Ukupni prihod", se.UkupniPrihod);
            se.AddStatisticsOption("Prihod od osnovne usluge", se.PrihodOdOsnovneUsluge);
            se.AddStatisticsOption("Prihod od dodatnih opcija", se.PrihodOdDodatnihOpcija);
        }

        /// <summary>
        /// Returns all the available statistics options.
        /// </summary>
        /// <returns>JsonStatisticsContract</returns>
        public JsonStatisticsContract StatisticsContract(User requestUser) {
            JsonStatisticsContract contract = new JsonStatisticsContract();
            try {
                contract.Status = JsonBasicResponse.OK;
                contract.SupportedStatistics = se.SupportedStatistics();
                int? orgId = requestUser.organization_id;
                using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                    var clients = ctx.Clients.Where(c => c.User.Any(u => u.Id == requestUser.Id || u.organization_id == orgId));
                    foreach (Client client in clients) {
                        JsonClient jsonClient = new JsonClient();
                        jsonClient.Load(client);
                        contract.Clients.Add(jsonClient);
                    }
                    /* samo poslovni */
                    var categories = ctx.Categories.Where(c => c.ParentId == 1)
                        .ToList()
                        .Where(c => c.IsVisibleFor(requestUser))
                        .OrderBy(c => c.Name);
                    /* zašto dva where? Prvi se direktno prevodi u SQL, a drugi se izvodi na list kolekcijom */
                    /* budući da se radi o custom funkciji (IsVisibleFor), LINQ to ne zna direktno prevesti u SQL, */
                    /* ali zna s time raditi kada se pozove na kolekcijom. */
                    foreach (Category category in categories) {
                        JsonCategory jsonCategory = new JsonCategory();
                        jsonCategory.LoadPartial(category);
                        contract.InitialCategories.Add(jsonCategory);
                    }
                }
            }
            catch (Exception) {
                contract.Status = JsonBasicResponse.ERROR;
            }
            return contract;
        }

        /// <summary>
        /// Gets a specific selected statistic
        /// </summary>
        /// <returns>Returns a string in xml format, prepared for the client graph</returns>
        public JsonStatisticsResponse GetXmlStatistics(string data, User requestUser) {
            JsonStatisticsResponse response = new JsonStatisticsResponse();
            try {
                JsonStatisticsRequest request = DeserializeRequest(data, requestUser);
                try {
                    response = se.GetXmlStatistics(request);
                }
                catch (Exception ex) {
                    response.Status = JsonBasicResponse.ERROR;
                    response.ErrorText = "Greška prilikom računanja statistike: " + ex.Message;
                }
            }
            catch (ArgumentException ex) {
                response.Status = JsonBasicResponse.ERROR;
                response.ErrorText = "Neispravan JSON: " + ex.Message;
            }
            catch (Exception ex) {
                response.Status = JsonBasicResponse.ERROR;
                response.ErrorText = "Nepoznata greška: " + ex.Message;
            }
            return response;
        }
        /// <summary>
        /// Gets a specific selected statistic
        /// </summary>
        /// <returns>Returns a string in json format, prepared for the web graph</returns>
        public JsonStatisticsResponse GetJsonStatistics(string data, User requestUser) {
            JsonStatisticsResponse response = new JsonStatisticsResponse();
            try {
                JsonStatisticsRequest request = DeserializeRequest(data, requestUser);
                try {
                    response = se.GetJsonStatistics(request);
                }
                catch (Exception ex) {
                    response.Status = JsonBasicResponse.ERROR;
                    response.ErrorText = "Greška prilikom računanja statistike: " + ex.Message;
                }
            }
            catch (ArgumentException ex) {
                response.Status = JsonBasicResponse.ERROR;
                response.ErrorText = "Neispravan JSON: " + ex.Message;
            }
            catch (Exception ex) {
                response.Status = JsonBasicResponse.ERROR;
                response.ErrorText = "Nepoznata greška: " + ex.Message;
            }
            return response;
        }

        private JsonStatisticsRequest DeserializeRequest(string jsonRequest, User requestUser) {
            JsonStatisticsRequest request = null;
            JavaScriptSerializer serializator = new JavaScriptSerializer();
            request = serializator.Deserialize<JsonStatisticsRequest>(jsonRequest);
            request.UserId = requestUser.Id;
            return request;
        }
    }
}