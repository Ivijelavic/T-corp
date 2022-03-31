using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace TCorp.Components {
    public static class ConstantsComponent {
        private const string DECIMAL_SEPARATOR_STRING = ",";
        public const decimal PDV = 1.25M;
        public static readonly NumberFormatInfo DECIMAL_SEPARATOR = new NumberFormatInfo() { NumberDecimalSeparator = DECIMAL_SEPARATOR_STRING };
    }
}