using System;
using TCorp.Components;

namespace TCorp.EntityFramework {
    public partial class Session {
        public void Load(User user) {
            CryptoComponent c = new CryptoComponent();
            this.AccessToken = c.GenerateAccessToken(user);
            this.Created = DateTime.Now;
            this.ValidUntil = null;
            this.user_id = user.Id;
            this.Owner = user;
        }
    }
}
