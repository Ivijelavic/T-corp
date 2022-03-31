using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TCorp.JsonResponseModels {
    public class JsonAdditionalOptions {
        public const string OK = "OK";
        public const string ERROR = "error";

        public string Status { get; set; }
        public List<JsonAdditionalOption> AdditionalOptions { get; set; }
        public JsonAdditionalOptions() {
            AdditionalOptions = new List<JsonAdditionalOption>();
        }
    }
}