using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Security;
using System.Web;
using System.Web.Script.Serialization;
using TCorp.EntityFramework;
using TCorp.JsonIncomingModels.Racun;
using TCorp.JsonResponseModels;
using TCorp.Logger;

namespace TCorp.Components {
    /// <summary>
    /// Handles all things receipt related
    /// </summary>
    public class ReceiptComponent {
        private static readonly BaseLogger logger = new DbLogger();

        #region Save Receipt
        /// <summary>
        /// Saves the receipt to the database
        /// </summary>
        /// <param name="data">The receipt in json</param>
        /// <param name="pdfName">The name of the pdf file assosiated with this receipt</param>
        /// <param name="requestUser">The User saving the receipt</param>
        /// <param name="pdfId">The Id of the created pdf. Note out(!)</param>
        /// <returns>JsonBasicResponse</returns>
        public JsonBasicResponse SaveReceipt(string data, string pdfName, User requestUser, out int pdfId) {
            JsonBasicResponse response = new JsonBasicResponse();
            int tempPdfId = 0;
            try {
                JsonRacun racun = DeserializeRacun(data);
                response = SaveReceipt(racun, pdfName, requestUser, out tempPdfId);
            }
            catch (ArgumentException ex) {
                response.Status = JsonBasicResponse.ERROR;
                response.Data = "Neispravan JSON: " + ex.Message;
            }
            catch (Exception ex) {
                response.Status = JsonBasicResponse.ERROR;
                response.Data = "Greška prilikom spremanja, greška s bazom podataka: " + ex.Message;
            }
            pdfId = tempPdfId;
            return response;
        }

        private JsonBasicResponse SaveReceipt(JsonRacun racun, string pdfName, User requestUser, out int pdfId) {
            if (requestUser == null) {
                logger.Log(null, BaseLogger.WebAction.SecurityException, "Invalid (null) user entered ServeRequest method!");
                throw new SecurityException("Invalid (null) user entered private SaveReceipt method!");
            }
            JsonBasicResponse response = new JsonBasicResponse();
            Receipt r = new Receipt();
            r.Load(racun, pdfName, requestUser);
            using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                ctx.Receipts.Add(r);
                ctx.SaveChanges();
            }
            pdfId = r.Id;
            response.Status = JsonBasicResponse.OK;
            response.Data = "Zaduženje evidentirano";
            return response;
        }

        #endregion

        #region Save Or Update Template

        public JsonBasicResponse SaveOrUpdateTemplate(string data, User requestUser) {
            JsonBasicResponse response = new JsonBasicResponse();
            try {
                JsonRacun racun = DeserializeRacun(data);
                response = SaveOrUpdateTemplate(racun, requestUser);
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

        private JsonBasicResponse SaveOrUpdateTemplate(JsonRacun racun, User requestUser) {
            if (requestUser == null) {
                logger.Log(null, BaseLogger.WebAction.SecurityException, "Invalid (null) user entered ServeRequest method!");
                throw new SecurityException("Invalid (null) user entered private SaveTemplate method!");
            }
            JsonBasicResponse response = new JsonBasicResponse();
            using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                ReceiptTemplate rt = ctx.ReceiptTemplate.Include(t => t.ReceiptTemplateBasket).SingleOrDefault(r => r.Name == racun.Naziv && r.user_id == requestUser.Id);
                if (rt == null) {
                    rt = new ReceiptTemplate();
                    rt.Load(racun, requestUser);
                    ctx.ReceiptTemplate.Add(rt);
                }
                else {
                    ctx.Database.ExecuteSqlCommand("DELETE FROM ReceiptTemplateBasket WHERE receiptTemplate_id = " + rt.Id);
                    rt.Load(racun, requestUser);
                }
                ctx.SaveChanges();
            }
            response.Status = JsonBasicResponse.OK;
            response.Data = "Predložak pohranjen";
            return response;
        }

        #endregion

        #region List Template

        public List<JsonRacun> ListTemplate(User requestUser) {
            List<JsonRacun> response = new List<JsonRacun>();
            using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                List<ReceiptTemplate> templates = ctx.ReceiptTemplate.Where(rt => rt.user_id == requestUser.Id).ToList();
                foreach (ReceiptTemplate template in templates) {
                    JsonRacun jr = new JsonRacun();
                    jr.LoadHeader(template);
                    response.Add(jr);
                }
            }
            return response;
        }

        #endregion

        #region Get Template

        public dynamic GetTemplate(string data, User requestUser) {
            dynamic response;
            try {
                int id = int.Parse(data);
                response = GetTemplate(id, requestUser);
            }
            catch (ArgumentException ex) {
                response = new JsonBasicResponse();
                response.Status = JsonBasicResponse.ERROR;
                response.Data = "Neispravan JSON: " + ex.Message;
            }
            catch (Exception ex) {
                response = new JsonBasicResponse();
                response.Status = JsonBasicResponse.ERROR;
                response.Data = "Greška prilikom dohvata predloška iz baze podataka: " + ex.Message;
            }
            return response;
        }

        private JsonRacun GetTemplate(int id, User requestUser) {
            JsonRacun response = new JsonRacun();
            using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                ReceiptTemplate savedTemplate = ctx.ReceiptTemplate.AsNoTracking().
                    Include(t => t.ReceiptTemplateBasket).
                    Include(t => t.ReceiptTemplateBasket.
                        Select(r => r.Category)).
                    Include(t => t.ReceiptTemplateBasket.
                        Select(r => r.Category.Category_Role_MaxDiscount)).
                        SingleOrDefault(rt => rt.Id == id);
                List<ReceiptTemplateBasket> actualBasket = savedTemplate.ReceiptTemplateBasket.Where(rtb => rtb.Category.IsVisibleFor(requestUser)).ToList();
                int savedItems = savedTemplate.ReceiptTemplateBasket.Count;
                int actualItems = actualBasket.Count;
                savedTemplate.ReceiptTemplateBasket = actualBasket;
                if (savedTemplate == null) {
                    throw new ArgumentException("Neispravan id predloška: " + id.ToString());
                }
                else {
                    if (savedTemplate.user_id == requestUser.Id) {
                        response.Load(savedTemplate, requestUser);
                    }
                    else {
                        throw new SecurityException("Pokušali ste dohvatiti predložak kojeg niste Vi stvorili. U slučaju greške kontaktirajte administratora.");
                    }
                }
                if (savedItems != actualItems) {
                    response.TemplateCorrupt = true;
                }
                var dodatneOpcijeLookup = ctx.AdditionalOptions.AsNoTracking().ToList();
                foreach (JsonPaket jp in response.Kosarica.Paket) {
                    int categoryId = int.Parse(jp.CategoryId);
                    var dodatnaOpcija = dodatneOpcijeLookup.Find(a => a.category_id == categoryId && a.Name == jp.NazivVrsteRada);
                    if (dodatnaOpcija == null) {
                        jp.DodatneOpcijeCorrupt = false;
                    }
                    else {
                        DateTime? compareDate = (dodatnaOpcija.DateModified == null) ? dodatnaOpcija.DateCreated : dodatnaOpcija.DateModified;
                        jp.DodatneOpcijeCorrupt = (compareDate >= savedTemplate.DateCreated) ? true : false;
                    }
                }
            }
            return response;
        }

        #endregion

        #region Remove Template

        public JsonBasicResponse RemoveTemplate(string data, User requestUser) {
            JsonBasicResponse response = new JsonBasicResponse();
            try {
                int id = int.Parse(data);
                response = RemoveTemplate(id, requestUser);
            }
            catch (ArgumentException ex) {
                response.Status = JsonBasicResponse.ERROR;
                response.Data = "Neispravan JSON: " + ex.Message;
            }
            catch (Exception ex) {
                response.Status = JsonBasicResponse.ERROR;
                response.Data = "Greška prilikom brisanja predloška: " + ex.Message;
            }
            return response;
        }

        private JsonBasicResponse RemoveTemplate(int id, User requestUser) {
            if (requestUser == null) {
                logger.Log(null, BaseLogger.WebAction.SecurityException, "Invalid (null) user entered ServeRequest method!");
                throw new SecurityException("Invalid (null) user entered private RemoveTemplate method!");
            }
            /* pretpostavi najgore */
            JsonBasicResponse response = new JsonBasicResponse();
            using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                ReceiptTemplate rt = ctx.ReceiptTemplate.SingleOrDefault(t => t.Id == id);
                if (rt == null) {
                    throw new Exception("Nema predloška s tim identifikacijskim brojem");
                }
                else {
                    if (rt.user_id == requestUser.Id) {
                        ctx.ReceiptTemplate.Remove(rt);
                        ctx.SaveChanges();
                    }
                    else {
                        throw new SecurityException("Pokušali ste izbrisati predložak koji ne pripada vama. Ova akcija je zabilježena i prijavljena administratoru");
                    }
                }
            }
            response.Status = JsonBasicResponse.OK;
            response.Data = "Predložak izbrisan";
            return response;
        }

        #endregion

        #region Remove All Templates

        public JsonBasicResponse RemoveAllTemplates(User requestUser) {
            JsonBasicResponse response = new JsonBasicResponse();
            try {
                response = RemoveAllTemplates(requestUser.Id);
            }
            catch (ArgumentException ex) {
                response.Status = JsonBasicResponse.ERROR;
                response.Data = "Neispravan JSON: " + ex.Message;
            }
            catch (Exception ex) {
                response.Status = JsonBasicResponse.ERROR;
                response.Data = "Greška prilikom brisanja predloška: " + ex.Message;
            }
            return response;
        }

        private JsonBasicResponse RemoveAllTemplates(int user_id) {
            JsonBasicResponse response = new JsonBasicResponse();
            using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                List<ReceiptTemplate> templates = ctx.ReceiptTemplate.Where(t => t.user_id == user_id).ToList();
                foreach (ReceiptTemplate rt in templates) {
                    ctx.ReceiptTemplate.Remove(rt);
                }
                ctx.SaveChanges();
            }
            response.Status = JsonBasicResponse.OK;
            response.Data = "Svi predlošci su izbrisani.";
            return response;
        }

        #endregion

        public JsonRacun DeserializeRacun(string data) {
            JsonRacun racunObject = null;
            JavaScriptSerializer serializator = new JavaScriptSerializer();
            racunObject = serializator.Deserialize<JsonRacun>(data);
            return racunObject;
        }
    }
}