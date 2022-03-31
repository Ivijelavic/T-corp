using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TCorp.EntityFramework;
using TCorp.JsonResponseModels;

namespace TCorp.Components {
    /// <summary>
    /// Handles getting Additional Options (dodatne opcije) from the server
    /// </summary>
    public class AdditionalDataFormComponent {

        /// <summary>
        /// Gets the JsonAdditionalOptions object
        /// </summary>
        /// <param name="data">The id of the category, passed as a string</param>
        /// <returns>Returns JsonAdditionalOptions which contain all possible AdditionalOptions</returns>
        public JsonAdditionalOptions GetForm(string data) {
            JsonAdditionalOptions response = new JsonAdditionalOptions();
            try {
                int categoryId = int.Parse(data);
                using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                    List<AdditionalOptions> additionalOptions = ctx.AdditionalOptions.AsNoTracking().Where(a => a.category_id == categoryId).ToList();
                    if (additionalOptions.Count == 0) {
                        response.Status = JsonAdditionalOptions.ERROR;
                        response.AdditionalOptions = null;
                    }
                    else {
                        List<JsonAdditionalOption> jsonAdditionalOptions = new List<JsonAdditionalOption>();
                        foreach (AdditionalOptions ao in additionalOptions) {
                            JsonAdditionalOption jao = new JsonAdditionalOption();
                            jao.Name = ao.Name;
                            jao.Data = ao.Data;
                            jsonAdditionalOptions.Add(jao);
                        }
                        response.Status = JsonAdditionalOptions.OK;
                        response.AdditionalOptions = jsonAdditionalOptions;
                    }
                }
            }
            catch (Exception) {
                response.Status = JsonAdditionalOptions.ERROR;
                response.AdditionalOptions = null;
            }
            return response;
        }
    }
}