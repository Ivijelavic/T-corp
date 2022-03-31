using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TCorp.EntityFramework {
    public partial class Device {
        public string DisplayName {
            get {
                return String.Format("{0}", this.Name);
            }
        }

        public Device Instance {
            get {
                return this;
            }
        }
    }
}