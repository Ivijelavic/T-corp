using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TCorp.Components;
using TCorp.EntityFramework;
using TCorp.JsonIncomingModels.Racun;

namespace TCorp.ViewModels.Pdf { 
    /// <summary>
    /// Holds the category descriptions that will be outputed in the pdf receipt
    /// </summary>
    public class CategoryViewModel {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}