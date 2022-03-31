using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Security;
using System.Web;
using System.Web.Script.Serialization;
using TCorp.EntityFramework;
using TCorp.JsonIncomingModels.Email;
using TCorp.JsonIncomingModels.Racun;
using TCorp.JsonResponseModels;
using TCorp.JsonResponseModels.Ostalo;
using TCorp.JsonResponseModels.Statistika;
using TCorp.Logger;
using TCorp.Models;

namespace TCorp.Components {
    /// <summary>
    /// Class responsible for routing and answering requests. Clients that enter this
    /// class are assumed to have a valid token. All token validation should be handled in the
    /// ServiceController class.
    /// </summary>
    public class ApiComponent {
        private static readonly BaseLogger logger = new DbLogger();
        /// <summary>
        /// Switcher method that routes incoming get requests
        /// </summary>
        /// <param name="request">The resource requested</param>
        /// <param name="requestUser">User that requested the resource</param>
        public dynamic ServeGetRequest(string request, User requestUser) {
            if (requestUser == null) {
                logger.Log(null, BaseLogger.WebAction.SecurityException, "Invalid (null) user entered ServeRequest method!");
                throw new SecurityException("Invalid (null) user entered ServeRequest method!");
            }
            dynamic response = null;
            switch (request) {
                case "Kategorije":
                    response = GetCategoriesForUser(requestUser);
                    break;
                case "ListPredlozak":
                    response = ListTemplates(requestUser);
                    break;
                case "IzbrisiSvePredloske":
                    response = RemoveAllTemplates(requestUser);
                    break;
                case "ListKlijent":
                    response = ListClients(requestUser);
                    break;
                case "KolekcijaKlijenata":
                    response = GetClientCollection(requestUser);
                    break;
                case "ZatraziPromjenu":
                    response = RequestChange(requestUser);
                    break;
                case "Verzija":
                    response = GetVersion(requestUser);
                    break;
                case "Statistika":
                    response = GetStatisticsContract(requestUser);
                    break;
                default:
                    response = new JsonBasicResponse();
                    response.Status = JsonBasicResponse.ERROR;
                    response.Data = "Invalid request method";
                    logger.Log(requestUser.Id, BaseLogger.WebAction.InvalidRequest, "GetData?request=" + request);
                    break;
            }
            return response;
        }
        /// <summary>
        /// Switcher method that routes incoming post requests
        /// </summary>
        /// <param name="request">The resource requested</param>
        /// <param name="requestUser">User that requested the resource</param>
        public dynamic ServeSendRequest(string request, string data, User requestUser) {
            if (requestUser == null) {
                logger.Log(null, BaseLogger.WebAction.SecurityException, "Invalid (null) user entered ServeRequest method!");
                throw new SecurityException("Invalid (null) user entered ServeSendRequest method!");
            }
            dynamic response = null;
            switch (request) {
                case "ZaduziRacun":
                    response = SaveAndEmailReceipt(data, requestUser);
                    break;
                case "SpremiPredlozak":
                    response = SaveTemplate(data, requestUser);
                    break;
                case "DohvatiPredlozak":
                    response = GetTemplate(data, requestUser);
                    break;
                case "DohvatiKlijenta":
                    response = GetClient(data, requestUser);
                    break;
                case "IzbrisiPredlozak":
                    response = RemoveTemplate(data, requestUser);
                    break;
                case "DohvatiObrazac":
                    response = GetForm(data, requestUser);
                    break;
                case "DodajKlijenta":
                    response = SaveClient(data, requestUser);
                    break;
                case "PreviewPdf":
                    response = PreviewPdf(data, requestUser);
                    break;
                case "Statistika":
                    response = GetStatisticsFor(data, requestUser);
                    break;
                case "PosaljiMail":
                    response = SendMail(data, requestUser);
                    break;
                default:
                    response = new JsonBasicResponse();
                    response.Status = JsonBasicResponse.ERROR;
                    response.Data = "Invalid request method";
                    logger.Log(requestUser.Id, BaseLogger.WebAction.InvalidRequest, "SendData?request=" + request);
                    break;
            }
            return response;
        }

        /// <summary>
        /// Sends an email with a pdf attachment to a client.
        /// </summary>
        /// <param name="data">The json containing information about the mail</param>
        /// <param name="requestUser">The user sending the email (not the client)</param>
        /// <returns>Status message in the form of a JsonBasicResponse</returns>
        private dynamic SendMail(string data, User requestUser) {
            JsonBasicResponse jbr = new JsonBasicResponse();
            try {
                MailComponent mc = new MailComponent();
                JsonEmail jsonEmail = mc.DeserializeEmail(data);
                EmailModel email = new EmailModel();
                email.Load(jsonEmail, requestUser);
                mc.SendMail(email);
                jbr.Status = JsonBasicResponse.OK;
                jbr.Data = "Mail poslan";
            }
            catch (Exception ex) {
                jbr.Status = JsonBasicResponse.ERROR;
                jbr.Data = ex.Message;
            }
            return jbr;
        }

        /// <summary>
        /// Gets all available statistical options and sends it to the user
        /// </summary>
        /// <param name="requestUser">The User requesting the info</param>
        /// <returns>Returns json</returns>
        private dynamic GetStatisticsContract(User requestUser) {
            StatisticsComponent sc = StatisticsComponent.Instance;
            JsonStatisticsContract response = sc.StatisticsContract(requestUser);
            return response;
        }

        /// <summary>
        /// Gets a specific type of statistic
        /// </summary>
        /// <param name="data">Details about the requested statistic</param>
        /// <param name="requestUser">The User requesting the statistical data</param>
        /// <returns>Returns json</returns>
        private dynamic GetStatisticsFor(string data, User requestUser) {
            StatisticsComponent sc = StatisticsComponent.Instance;
            JsonStatisticsResponse response = sc.GetXmlStatistics(data, requestUser);
            return response;
        }

        /// <summary>
        /// Gets the lattest version of the application
        /// </summary>
        /// <param name="requestUser">The requesting User</param>
        /// <returns>Returns the version in the form of a JsonBasicResponse</returns>
        [Obsolete("Never implemented client side - use Socket insted")]
        private JsonBasicResponse GetVersion(User requestUser) {
            /* User requestUser is reserved for future use */
            JsonBasicResponse response = new JsonBasicResponse();
            response.Status = JsonBasicResponse.OK;
            response.Data = ConfigurationManager.AppSettings["LatestVersion"];
            return response;

        }
        /// <summary>
        /// Notifies that the User has requested a change of his personal data
        /// </summary>
        /// <param name="requestUser">The User requesting the change</param>
        /// <returns>Returns JsonBasicResponse</returns>
        [Obsolete("Never actually used client side")]
        private dynamic RequestChange(User requestUser) {
            JsonBasicResponse response = new JsonBasicResponse();
            if (requestUser.RequestChangeDate == null) {
                using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                    User user = ctx.Users.Single(u => u.Id == requestUser.Id);
                    user.RequestChangeDate = DateTime.Now;
                    ctx.SaveChanges();
                }
                response.Status = JsonBasicResponse.OK;
                response.Data = "Uspješno ste zatražili promjenu";
            }
            else {
                response.Status = JsonBasicResponse.ERROR;
                response.Data = String.Format("Administrator pregledava vaš zahtjev koji je poslan: {0}", requestUser.RequestChangeDate);
            }
            return response;
        }

        /// <summary>
        /// Creates a preview pdf based on the data it is given
        /// </summary>
        /// <param name="data">The receipt in json</param>
        /// <param name="requestUser">The User requesting the pdf</param>
        /// <returns>Returns a http link to the pdf document in the JsonBasicResponse</returns>
        private dynamic PreviewPdf(string data, User requestUser) {
            PdfComponent pc = new PdfComponent();
            JsonBasicResponse response = pc.CreatePdf(data, 0, requestUser);
            return response;
        }
        /// <summary>
        /// Saves the receipt to the database and sends a copy of the pdf to the client
        /// </summary>
        /// <param name="data">The receipt, in json</param>
        /// <param name="requestUser">The User requesting the operation</param>
        /// <returns>Returns a status message in the form of a JsonBasicResponse</returns>
        private dynamic SaveAndEmailReceipt(string data, User requestUser) {
            PdfComponent pc = new PdfComponent();
            JsonBasicResponse response = pc.SendOfferToMail(data, requestUser);
            return response;
        }
        /// <summary>
        /// Creates a new client and saves him to the database
        /// </summary>
        /// <param name="data">The client data, in json</param>
        /// <param name="requestUser">The User requesting the operation</param>
        /// <returns>Returns a status message in the form of a JsonBasicResponse</returns>
        private JsonBasicResponse SaveClient(string data, User requestUser) {
            ClientComponent cc = new ClientComponent();
            JsonBasicResponse response = cc.SaveClient(data, requestUser);
            return response;
        }
        /// <summary>
        /// Gets a list of all clients, personal and shared
        /// </summary>
        /// <param name="requestUser">The User requesting his clients</param>
        /// <returns>Returns a list of clients in the form of a JsonClientCollection</returns>
        private JsonClientCollection GetClientCollection(User requestUser) {
            ClientComponent cc = new ClientComponent();
            JsonClientCollection response = cc.GetClientCollection(requestUser);
            return response;
        }
        /// <summary>
        /// Gets a list of all clients
        /// </summary>
        /// <param name="requestUser">The User requesting his clients</param>
        /// <returns>Returns a list of clients in the form of a List{JsonClient}</returns>
        private List<JsonClient> ListClients(User requestUser) {
            ClientComponent cc = new ClientComponent();
            List<JsonClient> response = cc.ListClients(requestUser);
            return response;
        }

        /// <summary>
        /// Gets details about a specific client
        /// </summary>
        /// <param name="data">The client Id</param>
        /// <param name="requestUser">The requesting User</param>
        /// <returns>Returns client details in JsonClient</returns>
        private JsonClient GetClient(string data, User requestUser) {
            ClientComponent cc = new ClientComponent();
            JsonClient client = cc.GetClient(data, requestUser);
            return client;
        }
        /// <summary>
        /// Gets all additional options for a specific category
        /// </summary>
        /// <param name="data">The category Id</param>
        /// <param name="requestUser">The requesting User</param>
        /// <returns></returns>
        private JsonAdditionalOptions GetForm(string data, User requestUser) {
            AdditionalDataFormComponent fc = new AdditionalDataFormComponent();
            JsonAdditionalOptions response = fc.GetForm(data);
            return response;
        }

        /// <summary>
        /// Removes all templates
        /// </summary>
        /// <param name="requestUser">The requesting User</param>
        /// <returns>JsonBasicResponse</returns>
        private JsonBasicResponse RemoveAllTemplates(User requestUser) {
            //logger.Log(requestUser.Id, BaseLogger.WebAction.RemoveAllTemplates, null);
            ReceiptComponent rc = new ReceiptComponent();
            JsonBasicResponse response = rc.RemoveAllTemplates(requestUser);
            return response;
        }

        /// <summary>
        /// Removes a specific template
        /// </summary>
        /// <param name="data">Template Id</param>
        /// <param name="requestUser">The requesting User</param>
        /// <returns>JsonBasicResponse</returns>
        private JsonBasicResponse RemoveTemplate(string data, User requestUser) {
            //logger.Log(requestUser.Id, BaseLogger.WebAction.RemoveTemplate, null);
            ReceiptComponent rc = new ReceiptComponent();
            JsonBasicResponse response = rc.RemoveTemplate(data, requestUser);
            return response;
        }
        /// <summary>
        /// Gets a specific template
        /// </summary>
        /// <param name="data">Template Id</param>
        /// <param name="requestUser">The requesting User</param>
        /// <returns>Returns the entire receipt in the form of a JsonRacun</returns>
        private dynamic GetTemplate(string data, User requestUser) {
            //logger.Log(requestUser.Id, BaseLogger.WebAction.GetTemplate, null);
            ReceiptComponent rc = new ReceiptComponent();
            dynamic response = rc.GetTemplate(data, requestUser);
            return response;
        }

        /// <summary>
        /// Gets all saved templates for a user
        /// </summary>
        /// <param name="requestUser">The requesting user</param>
        /// <returns>List{JsonRacun}, but loaded only with their names. To get details about a template use GetTemplate</returns>
        private List<JsonRacun> ListTemplates(User requestUser) {
            //logger.Log(requestUser.Id, BaseLogger.WebAction.ListTemplates, null);
            ReceiptComponent rc = new ReceiptComponent();
            List<JsonRacun> response = rc.ListTemplate(requestUser);
            return response;
        }
        /// <summary>
        /// Saves a receipt as a new template
        /// </summary>
        /// <param name="data">The json receipt along with the name of the template</param>
        /// <param name="requestUser">The requesting User</param>
        /// <returns>JsonBasicResponse</returns>
        private JsonBasicResponse SaveTemplate(string data, User requestUser) {
            //logger.Log(requestUser.Id, BaseLogger.WebAction.SpremiPredlozak, null);
            ReceiptComponent rc = new ReceiptComponent();
            JsonBasicResponse response = rc.SaveOrUpdateTemplate(data, requestUser);
            return response;
        }

        /// <summary>
        /// Returns the initial json, containing all the categories the client can see, prices, contracts, etc.
        /// </summary>
        /// <param name="requestUser">The requesting User</param>
        /// <returns>Returns JsonCategoryResponse which contains a whole lot of data</returns>
        private JsonCategoryResponse GetCategoriesForUser(User requestUser) {
            logger.Log(requestUser.Id, BaseLogger.WebAction.RequestAll, null);
            CategoryComponent category = new CategoryComponent();
            JsonCategoryResponse response = category.GetCategoriesForUser(requestUser);
            return response;
        }
    }
}
