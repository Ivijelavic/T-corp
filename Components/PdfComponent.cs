using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;
using TCorp.EntityFramework;
using TCorp.JsonIncomingModels.Racun;
using TCorp.JsonResponseModels;

namespace TCorp.Components {
    /// <summary>
    /// Handles the pdf generation
    /// </summary>
    public class PdfComponent {
        /* Instaliraj wkhtmltopdf. Ako ga ne instaliraš u Program Files, promijeni ovdje link
         * Instaliraj fontove.
         * Stvori virtualne direktorije, PdfRepository i PdfGeneratorCore.
         * Prekopiraj fajlove u core.
         * Pokreni.
         * Moli se da radi. */
        private static readonly string LOCATION = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);
        private static readonly string HOST = String.Format("{0}{1}", LOCATION, (LOCATION.Substring(0, 5) == "https" ? "/TCorp" : String.Empty));
        private static readonly string PDF_CORE_FOLDER = HttpContext.Current.Server.MapPath("~/PdfGeneratorCore");
        private static readonly string PDF_REPO_FOLDER = HttpContext.Current.Server.MapPath("~/PdfRepository");

        /// <summary>
        /// Creates a pdf.
        /// </summary>
        /// <param name="racun">The receipt that will be generating the pdf, json formatted</param>
        /// <param name="pdfId">The Id of the pdf</param>
        /// <param name="requestUser">The User generating the pdf</param>
        /// <returns>Returns the link to the pdf file in the form of a JsonBasicResponse</returns>
        public JsonBasicResponse CreatePdf(string racun, int pdfId, User requestUser) {
            JsonBasicResponse response = new JsonBasicResponse();
            ReceiptComponent rc = new ReceiptComponent();
            try {
                response = GeneratePdf(racun, pdfId, requestUser);
            }
            catch (ArgumentException ex) {
                response.Status = JsonBasicResponse.ERROR;
                response.Data = "Neispravan JSON: " + ex.Message;
            }
            catch (Exception ex) {
                response.Status = JsonBasicResponse.ERROR;
                response.Data = "Greška prilikom generiranja pdf-a: " + ex.Message;
            }
            return response;
        }
        /// <summary>
        /// Creates a pdf offer and emails it to a client
        /// </summary>
        /// <param name="data">The reciept in json</param>
        /// <returns>JsonBasicResponse</returns>
        public JsonBasicResponse SendOfferToMail(string data, User requestUser) {
            JsonBasicResponse response = new JsonBasicResponse();
            ReceiptComponent rc = new ReceiptComponent();
            MailComponent mc = new MailComponent();
            try {
                int pdfId;
                string pdfName = GeneratePdfName();
                JsonRacun racun = rc.DeserializeRacun(data);
                rc.SaveReceipt(data, pdfName, requestUser, out pdfId);
                var tuple = WkHtmlToPdf(data, pdfId, pdfName, requestUser);
                string fileLocation = tuple.Item2;
                Client client;
                using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                    int clientId = int.Parse(racun.ClientId);
                    client = ctx.Clients.AsNoTracking().Single(c => c.Id == clientId);
                }
                mc.SendOffer(requestUser, client, pdfId, fileLocation);
                response.Status = JsonBasicResponse.OK;
                response.Data = "Mail poslan";
            }
            catch (ArgumentException ex) {
                response.Status = JsonBasicResponse.ERROR;
                response.Data = "Neispravan JSON: " + ex.Message;
            }
            catch (Exception ex) {
                response.Status = JsonBasicResponse.ERROR;
                response.Data = "Greška prilikom generiranja pdf-a: " + ex.Message;
            }
            return response;
        }

        private JsonBasicResponse GeneratePdf(string racun, int pdfId, User requestUser) {
            JsonBasicResponse response = new JsonBasicResponse();
            response.Status = JsonBasicResponse.OK;
            string pdfName = String.Format("Temp_{0}", this.GeneratePdfName());
            response.Data = WkHtmlToPdf(racun, pdfId, pdfName, requestUser).Item1;
            return response;
        }

        /// <summary>
        /// WkHtmlToPdf magic happens here.
        /// </summary>
        /// <param name="racun">The receipt, json formatted</param>
        /// <param name="pdfId">The Pdf Id</param>
        /// <param name="pdfName">The name of the pdf file</param>
        /// <param name="requestUser">The requesting User</param>
        /// <returns></returns>
        private Tuple<string, string> WkHtmlToPdf(string racun, int pdfId, string pdfName, User requestUser) {
            /* majko isusa koji hardcoded quick fix s ovim tuple */
            /* kako li ću mirno spavati nakon ovoga? */
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WorkingDirectory = PDF_CORE_FOLDER;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = "C:/Program Files/wkhtmltopdf/wkhtmltopdf.exe";
            string httpPayload = String.Format("{0}/Pdf/Body", HOST);
            string pdfPath = String.Format("{0}/{1}", PDF_REPO_FOLDER, pdfName);
            string header = "header.html";
            string footer = "footer.html";
            string cover = String.Format("{0}/Pdf/Cover?Id={1}", HOST, pdfId);
            string marginTop = "35mm";
            string marginBottom = "25mm";
            string xslStyleSheet = "toc.xsl";
            string headerSpacing = "10";
            string footerSpacing = "10";
            byte[] racunByteArray = Encoding.UTF8.GetBytes(racun);
            string base64Json = Convert.ToBase64String(racunByteArray);
            string jsonRacun = String.Format(@"base64Data ""{0}""", HttpUtility.UrlEncode(base64Json));
            string userId = String.Format("userId {0}", requestUser.Id);
            string args = String.Format(@"--post {0} --post {1} --header-html {8} --header-spacing {2} --footer-html {3} --footer-spacing {4} --margin-top {5} --margin-bottom {6} cover {7} --header-html {8} toc --xsl-style-sheet {9} {10} ""{11}""",
                jsonRacun, userId, headerSpacing, footer, footerSpacing, marginTop, marginBottom, cover, header, xslStyleSheet, httpPayload, pdfPath);
            startInfo.Arguments = args;
            Process p = Process.Start(startInfo);
            p.WaitForExit();
            int result = p.ExitCode;
            if (result == 0 || result == 1) {
                string webLocation = String.Format("{0}/PdfRepository/{1}", HOST, pdfName);
                string localLocation = String.Format("{0}/{1}", PDF_REPO_FOLDER, pdfName);
                return new Tuple<string, string>(webLocation, localLocation);
            }
            else {
                throw new Exception("Error code: " + result.ToString());
            }
        }

        private string GeneratePdfName() {
            return Guid.NewGuid().ToString().Replace("-", "") + ".pdf";
        }
    }
}