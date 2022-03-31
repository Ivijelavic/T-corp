using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using TCorp.Components;
using TCorp.EntityFramework;
using TCorp.JsonIncomingModels.Racun;
using TCorp.ViewModels;
using TCorp.ViewModels.Pdf;

namespace TCorp.Controllers {
    public class PdfController : Controller {

        public ActionResult Body(string base64Data, int userId) {
            var base64ByteArray = Convert.FromBase64String(HttpUtility.UrlDecode(base64Data));
            string data = Encoding.UTF8.GetString(base64ByteArray);
            try {
                ReceiptComponent rc = new ReceiptComponent();
                JsonRacun racun = rc.DeserializeRacun(data);
                PdfViewModel pvm = new PdfViewModel(userId);
                pvm.LoadFromRacun(racun);
                pvm.Timestamp = DateTime.Now;
                return View(pvm);
            }
            catch (Exception ex) {
                throw new ArgumentException("Invalid json: " + ex.Message);
            }
        }

        public ActionResult Cover(int Id) {
            ViewBag.OfferId = Id;
            return View();
        }
    }
}
