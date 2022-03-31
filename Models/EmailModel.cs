using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TCorp.EntityFramework;
using TCorp.JsonIncomingModels.Email;

namespace TCorp.Models {
    public class EmailModel {
        public string ToEmail { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string FromName { get; set; }
        public string Bcc { get; set; }

        public void Load(JsonEmail jsonEmail, User requestUser) {
            this.Subject = jsonEmail.Subject;
            this.Body = jsonEmail.Body;
            this.FromName = requestUser.DisplayName;
            if (jsonEmail.ForTechSupport) {
                var config = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~/");
                var techSupport = config.AppSettings.Settings["TechSupportEmail"];
                this.ToEmail = techSupport.Value;
            }
            else {
                using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                    this.ToEmail = ctx.Clients.AsNoTracking().Single(c => c.Id == jsonEmail.ClientId).Email;
                }
            }
            if (requestUser.BccField == null || requestUser.BccField.Trim() == String.Empty) {
                this.Bcc = String.Empty;
            }
            else {
                this.Bcc = requestUser.BccField;
            }
        }
    }
}