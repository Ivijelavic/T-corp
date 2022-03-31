namespace TCorp.Logger {
    /// <summary>
    /// Abstract class that provides all the possible Actions that are to be logged.
    /// </summary>
    public abstract class BaseLogger {
        public enum WebAction {
            TokenCreated = 1,
            WrongUsername = 2,
            WrongPassword = 3,
            RequestAll = 4,
            AdminLogin = 5,
            AdminLogout = 6,
            TokenErased = 7,
            BannedLogin = 8,
            TimedOutLogin = 9,
            InvalidRequest = 10,
            NullToken = 11,
            InvalidToken = 12,
            UserLogin = 13,
            UserLogout = 14,
            FailedLogout = 15,
            SecurityException = 16,
            IntruderInCPanel = 17,
            ZaduziRacun = 18
        }
        public abstract void Log(int? userId, WebAction action, string desc);
    }
}