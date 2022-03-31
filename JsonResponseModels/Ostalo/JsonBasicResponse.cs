namespace TCorp.JsonResponseModels {
    public class JsonBasicResponse {
        public const string OK = "OK";
        public const string ERROR = "error"; 

        public string Status { get; set; }
        public string Data { get; set; }
    }
}