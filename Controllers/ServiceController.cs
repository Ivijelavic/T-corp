using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using TCorp.Components;
using TCorp.EntityFramework;
using TCorp.JsonIncomingModels.Racun;
using TCorp.JsonResponseModels;
using TCorp.Logger;

namespace TCorp.Controllers {
    /// <summary>
    /// API class exposed to clients.
    /// Token validation should take place here
    /// </summary>
    public class ServiceController : CoreWebController {
        public ActionResult Index() {
            return View();
        }
        /// <summary>
        /// Main method for serving get requests. Based upon the request
        /// returns the result only for a valid token
        /// </summary>
        /// <param name="token">Security token used to validate the request. If valid, token belongs to one person</param>
        /// <param name="request">The resource requested from the client</param>
        /// <returns>Returns in JSON either the resource requested or an invalid token message</returns>
        //[HttpPost]
        public ActionResult GetData(string token, string request) {
            dynamic jsonResponse = null;
            if (token == null) {
                logger.Log(null, BaseLogger.WebAction.NullToken, "GetData?request=" + request);
                jsonResponse = new List<JsonBasicResponse> {
                    new JsonBasicResponse {Status = "error", Data = "No token provided"}
                };
            }
            var requestUser = authComponent.GetTokenOwner(token);
            if (requestUser == null) {
                logger.Log(null, BaseLogger.WebAction.InvalidToken, "GetData?token=" + token + "&request=" + request);
                jsonResponse = new List<JsonBasicResponse> {
                    new JsonBasicResponse {Status = "error", Data = "Invalid token"}
                };
            }
            else {
                ApiComponent api = new ApiComponent();
                jsonResponse = api.ServeGetRequest(request, requestUser);
            }
            return Json(jsonResponse, JsonRequestBehavior.AllowGet);
        }

        //[HttpPost]
        public ActionResult SendData(string token, string request, string data) {
            dynamic jsonResponse = null;
            if (token == null) {
                logger.Log(null, BaseLogger.WebAction.NullToken, "SendData?request=" + request);
                jsonResponse = new JsonBasicResponse();
                jsonResponse.Status = JsonBasicResponse.ERROR;
                jsonResponse.Data = "No token provided";
            }
            var requestUser = authComponent.GetTokenOwner(token);
            if (requestUser == null) {
                logger.Log(null, BaseLogger.WebAction.InvalidToken, "SendData?token=" + token + "&request=" + request);
                jsonResponse = new JsonBasicResponse();
                jsonResponse.Status = JsonBasicResponse.ERROR;
                jsonResponse.Data = "Invalid token";
            }
            else {
                ApiComponent api = new ApiComponent();
                jsonResponse = api.ServeSendRequest(request, data, requestUser);
            }
            return Json(jsonResponse, JsonRequestBehavior.AllowGet);
        }
    }
}