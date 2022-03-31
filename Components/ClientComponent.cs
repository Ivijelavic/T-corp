using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using TCorp.EntityFramework;
using TCorp.JsonResponseModels;
using TCorp.JsonResponseModels.Ostalo;

namespace TCorp.Components {
    /// <summary>
    /// Handles all things client related
    /// </summary>
    public class ClientComponent {
        /// <summary>
        /// Lists all personal (not shared) clients for a specific user 
        /// </summary>
        /// <param name="requestUser">The requesting user</param>
        /// <returns>List{JsonClient}</returns>
        public List<JsonClient> ListClients(User requestUser) {
            List<JsonClient> response = new List<JsonClient>();
            using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                int? orgId = requestUser.organization_id;
                //foreach (Client c in ctx.Clients.Where(client => client.User.Any(u => u.Id == requestUser.Id || u.organization_id == orgId))) {
                foreach (Client c in ctx.Clients.Where(client => client.User.Any(u => u.Id == requestUser.Id))) {
                    JsonClient jc = new JsonClient();
                    jc.Load(c);
                    response.Add(jc);
                }
            }
            return response;
        }
        /// <summary>
        /// Get details about a specific client
        /// </summary>
        /// <param name="data">Client Id</param>
        /// <returns>JsonClient</returns>
        public JsonClient GetClient(string data, User requestUser) {
            JsonClient response = new JsonClient();
            int clientId = int.Parse(data);
            int? orgId = requestUser.organization_id;
            using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                //Client client = ctx.Clients.SingleOrDefault(c => c.User.Any(u => u.Id == requestUser.Id || u.organization_id == orgId) && c.Id == clientId);
                Client client = ctx.Clients.SingleOrDefault(c => c.User.Any(u => u.Id == requestUser.Id) && c.Id == clientId);
                response.Load(client);
                return response;
            }
        }
        /// <summary>
        /// Returns all shared and personal clients
        /// </summary>
        /// <returns>JsonClientCollection</returns>
        public JsonClientCollection GetClientCollection(User requestUser) {
            JsonClientCollection response = new JsonClientCollection();
            try {
                using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                    int? orgId = requestUser.organization_id;
                    //foreach (Client c in ctx.Clients.Where(client => client.User.Any(u => u.Id == requestUser.Id || u.organization_id == orgId))) {
                    foreach (Client c in ctx.Clients.Where(client => client.User.Any(u => u.Id == requestUser.Id))) {
                        JsonClient jc = new JsonClient();
                        jc.Load(c);
                        response.SharedClients.Add(jc);
                        if (c.User.Any(u => u.Id == requestUser.Id)) {
                            response.PersonalClients.Add(jc);
                        }
                    }
                }
            }
            catch (Exception ex) {
                response.Error = ex.Message; 
            }
            return response;
        }
        /// <summary>
        /// Adds a new client
        /// </summary>
        /// <param name="data">Client data</param>
        /// <returns>JsonBasicResponse</returns>
        public JsonBasicResponse SaveClient(string data, User requestUser) {
            JsonBasicResponse response = new JsonBasicResponse();
            try {
                Client client = DeserializeClient(data);
                using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                    User user = ctx.Users.Single(u => u.Id == requestUser.Id);
                    user.Client.Add(client);
                    ctx.Clients.Add(client);
                    ctx.SaveChanges();
                }
                response.Status = JsonBasicResponse.OK;
                response.Data = "Uspješno ste dodali klijenta";
            }
            catch (ArgumentException ex) {
                response.Status = JsonBasicResponse.ERROR;
                response.Data = "Neispravan JSON: " + ex.Message;
            }
            catch (Exception ex) {
                response.Status = JsonBasicResponse.ERROR;
                response.Data = "Greška prilikom spremanja, greška s bazom podataka: " + ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Create a Clinet object from a json formatted string
        /// </summary>
        /// <param name="data">The client json</param>
        /// <returns>Client</returns>
        private Client DeserializeClient(string data) {
            JavaScriptSerializer serializator = new JavaScriptSerializer();
            Client client = serializator.Deserialize<Client>(data);
            return client;
        }
    }
}