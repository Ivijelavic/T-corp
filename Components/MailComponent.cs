using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Script.Serialization;
using TCorp.EntityFramework;
using TCorp.JsonIncomingModels.Email;
using TCorp.Models;

namespace TCorp.Components {
    /// <summary>
    /// Handles all thing E-Mail related
    /// </summary>
    public class MailComponent {
        /// <summary>
        /// Sends a pdf offer to a client
        /// </summary>
        /// <param name="sender">The User sending the offer</param>
        /// <param name="recipient">The Client getting the offer</param>
        /// <param name="receiptId">The Id of receipt being sent</param>
        /// <param name="pdfFile">Name of the pdf file to attach</param>
        /// <returns>Boolean</returns>
        public bool SendOffer(User sender, Client recipient, int receiptId, string pdfFile) {
            try {
                var config = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~/");
                var fromAddressConfig = config.AppSettings.Settings["EmailAddressFrom"];
                var passwordConfig = config.AppSettings.Settings["EmailPassword"];
                var hostConfig = config.AppSettings.Settings["SMTP"];
                var portConfig = config.AppSettings.Settings["Port"];
                var sslConfig = config.AppSettings.Settings["SSL"];

                var fromAddress = new MailAddress(fromAddressConfig.Value, sender.DisplayName);
                string fromPassword = passwordConfig.Value;

                var smtp = new SmtpClient {
                    Host = hostConfig.Value,
                    Port = int.Parse(portConfig.Value),
                    EnableSsl = bool.Parse(sslConfig.Value),
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword),
                    Timeout = 20000
                };
                var message = new MailMessage();
                var to = new MailAddress(recipient.Email);
                message.To.Add(to);
                if (sender.BccField != null) {
                    foreach (string emailAddress in sender.BccField.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)) {
                        var bcc = new MailAddress(emailAddress);
                        message.Bcc.Add(bcc);
                    }
                }
                message.From = fromAddress;
                message.Subject = String.Format("Ponuda za {0}", recipient.DisplayName);
                message.Body = String.Format(
                    "Poštovani,{0}U privitku se nalazi ponuda Hrvatskog Telekoma d.d. br. {1}/{2}{0}U slučaju dodatnih pitanja, stojim na raspolaganju.{0}S poštovanjem,{0}{3}{0}{0}", "<br /><br />", receiptId, DateTime.Now.Year, sender.DisplayName);
                var pdfAttachment = new Attachment(pdfFile);
                pdfAttachment.Name = String.Format("{0}.pdf", message.Subject);
                message.Attachments.Add(pdfAttachment);
                message.IsBodyHtml = true;
                smtp.Send(message);
            }
            catch (Exception ex) {
                System.IO.File.AppendAllText("crashlog.txt", String.Format("[{0}] - {1}{2}", DateTime.Now, ex.Message, Environment.NewLine));
                return false;
            }
            return true;
        }
        /// <summary>
        /// Sends an email with no attachment. Used for emailing tech support.
        /// </summary>
        /// <param name="email">The EmailModel containing the necessary parameters</param>
        /// <returns></returns>
        public bool SendMail(EmailModel email) {
            try {
                var config = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~/");
                var fromAddressConfig = config.AppSettings.Settings["EmailAddressFrom"];
                var passwordConfig = config.AppSettings.Settings["EmailPassword"];
                var hostConfig = config.AppSettings.Settings["SMTP"];
                var portConfig = config.AppSettings.Settings["Port"];
                var sslConfig = config.AppSettings.Settings["SSL"];

                var fromAddress = new MailAddress(fromAddressConfig.Value, email.FromName);
                string fromPassword = passwordConfig.Value;
                string subject = email.Subject;
                string body = email.Body;

                var smtp = new SmtpClient {
                    Host = hostConfig.Value,
                    Port = int.Parse(portConfig.Value),
                    EnableSsl = bool.Parse(sslConfig.Value),
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword),
                    Timeout = 20000
                };
                var message = new MailMessage();
                foreach (string emailAddress in email.ToEmail.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)) {
                    var to = new MailAddress(emailAddress);
                    message.To.Add(to);
                }
                if (email.Bcc != null) {
                    foreach (string emailAddress in email.Bcc.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)) {
                        var bcc = new MailAddress(emailAddress);
                        message.Bcc.Add(bcc);
                    }
                }
                message.From = fromAddress;
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = true;
                smtp.Send(message);
            }
            catch (Exception ex) {
                System.IO.File.AppendAllText("crashlog.txt", String.Format("[{0}] - {1}{2}", DateTime.Now, ex.Message, Environment.NewLine));
                return false;
            }
            return true;
        }
        /// <summary>
        /// Creates a JsonEmail object from a json encoded string
        /// </summary>
        public JsonEmail DeserializeEmail(string data) {
            JsonEmail email = null;
            JavaScriptSerializer serializator = new JavaScriptSerializer();
            email = serializator.Deserialize<JsonEmail>(data);
            return email;
        }
    }
}