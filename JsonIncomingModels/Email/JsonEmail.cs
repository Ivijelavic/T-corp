using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TCorp.JsonIncomingModels.Email {
    public class JsonEmail {
        public bool ForTechSupport { get; set; }
        public int ClientId { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}