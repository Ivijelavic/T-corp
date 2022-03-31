using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using TCorp.EntityFramework;
using TCorp.JsonResponseModels;

namespace TCorp.Components {
    /// <summary>
    /// Fat class that handles all things related to the initial json that the client reads
    /// </summary>
    public class CategoryComponent {
        private const string ROOT_LINK = "<a href = \"./\">Root</a>";

        #region GetCategories

        static CategoryComponent() {

        }
        /// <summary>
        /// Loads each individual category object
        /// </summary>
        /// <returns>List{JsonCategory}</returns>
        private List<JsonCategory> GetAllCategories(User requestUser, List<Category> rawCategories, int? parentId) {
            List<JsonCategory> result = new List<JsonCategory>();
            foreach (Category category in rawCategories.Where(c => (parentId == null) ? c.ParentId.Equals(null) : c.ParentId == parentId)) {
                JsonCategory jcm = new JsonCategory();
                jcm.Name = category.Name;
                jcm.Prices = category.PricesToJson();
                jcm.Discounts = category.DiscountsToJson(requestUser);
                jcm.Children = GetAllCategories(requestUser, rawCategories, category.Id);
                jcm.Name = category.Name;
                jcm.CategoryId = category.Id;
                jcm.Info = category.Info;
                var discount = category.Category_Role_MaxDiscount.SingleOrDefault(cr => cr.role_id == requestUser.role_id);
                jcm.MaxDiscount = (discount == null) ? 0 : discount.MaxDiscount * 100; /* Jedino mjesto gdje server množi sa 100, ostalo dođe kao decimal */
                /* Tu je bio neki uvjet za dodavanje, ko će ga sad znat */
                jcm.InitialQuantity = category.InitialQuantity;
                jcm.InitialDiscount = category.InitialDiscount;
                if (category.Category_InStock == null) {
                    jcm.HasStockInfo = false;
                }
                else {
                    jcm.HasStockInfo = true;
                    jcm.QuantityInStock = category.Category_InStock.Quantity;
                }
                result.Add(jcm);
            }
            return result;
        }

        /// <summary>
        /// Loads the json and returns it to the api component
        /// </summary>
        /// <param name="requestUser">The requesting User</param>
        /// <returns>Returns the initial json that the client gets, JsonCategoryResponse</returns>
        public JsonCategoryResponse GetCategoriesForUser(User requestUser) {
            List<Category> rawCategories = null;
            using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                //OrderBy(Sort)
                rawCategories = ctx.Categories.AsNoTracking().
                    Include(c => c.Category_Role_MaxDiscount).
                    Include(c => c.Category_Price).
                    Include("Category_Price.Contract").
                    Include(c => c.PredefinedDiscount_Category).
                    Include("PredefinedDiscount_Category.PredefinedDiscount").
                    Include(c => c.Category_InStock).
                    Where(c => c.IsDeleted == false &&
                        c.IsHidden == false &&
                        c.AuthorizedRoles.Any(r => r.Id == requestUser.Role.Id))
                        .ToList();
            }
            JsonCategoryResponse jcr = new JsonCategoryResponse();
            jcr.CategoryTree = GetAllCategories(requestUser, rawCategories, null);
            jcr.CategoryList = GetAllLeafs(requestUser, jcr.CategoryTree);
            jcr.Contracts = GetAllContracts();
            jcr.Discounts = GetAllDiscounts(requestUser);
            jcr.Clusters = GetAllClusters();
            return jcr;
        }

        private List<string> GetAllClusters() {
            using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                var clusters = ctx.Device_TSpotApps_Price.AsNoTracking().Select(d => d.cluster).Distinct().OrderBy(c => c).ToList();
                int index = clusters.IndexOf("null");
                if (index >= 0) {
                    clusters[index] = "Nema clustera";
                }
                return clusters;
            }
        }

        private List<JsonDiscount> GetAllDiscounts(User requestUser) {
            List<JsonDiscount> result = new List<JsonDiscount>();
            using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                var discounts = ctx.PredefinedDiscount.AsNoTracking().ToList();
                foreach (var discount in discounts) {
                    JsonDiscount jd = new JsonDiscount();
                    jd.DiscountId = discount.Id.ToString();
                    jd.Name = discount.Name;
                    jd.DurationInMonths = discount.DurationInMonths;
                    result.Add(jd);
                }
            }
            return result;
        }

        private List<JsonContract> GetAllContracts() {
            List<JsonContract> result = new List<JsonContract>();
            using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                var contracts = ctx.Contract.AsNoTracking().ToList();
                foreach (var contract in contracts) {
                    JsonContract jc = new JsonContract();
                    jc.Name = contract.Name;
                    jc.ContractId = contract.Id.ToString();
                    result.Add(jc);
                }
            }
            return result;
        }

        /// <summary>
        /// Recursive function that goes through all the categories and grabs only the ones with no children
        /// </summary>
        private List<JsonCategory> GetAllLeafs(User requestUser, List<JsonCategory> categories) {
            List<JsonCategory> result = new List<JsonCategory>();
            foreach (JsonCategory js in categories) {
                if (js.Children.Count == 0) {
                    result.Add(js);
                }
                else {
                    result.AddRange(GetAllLeafs(requestUser, js.Children));
                }
            }
            return result;
        }

        #endregion

        /// <summary>
        /// Used to generate breadcrumbs 
        /// </summary>
        /// <param name="ctx">The database context</param>
        /// <param name="id">Id of the category. The method finds it's parent and continues recursively</param>
        /// <returns>Returns a list of links that represent breadcrumbs all the way to the currently selected category</returns>
        public LinkedList<string> GetAncestorsLinks(TCorpDbEntities ctx, int? id) {
            LinkedList<string> ancestors = new LinkedList<string>();
            while (id != null) {
                var baseCategory = ctx.Categories.AsNoTracking().SingleOrDefault(c => (id == null) ? c.Id.Equals(null) : c.Id == id);
                if (baseCategory == null) {
                    throw new ArgumentException("Invalid id");
                }
                string link = "<a href = \"" + baseCategory.Id + "\">" + baseCategory.Name + "</a>";
                ancestors.AddFirst(link);
                id = baseCategory.ParentId;
            }
            ancestors.AddFirst(ROOT_LINK);
            return ancestors;
        }

        public List<Category> GetRootCategories(TCorpDbEntities ctx) {
            return ctx.Categories.AsNoTracking().Where(c => c.ParentCategory == null).ToList();
        }

        public Category GetCategoryById(TCorpDbEntities ctx, int? id) {
            return ctx.Categories
                .AsNoTracking()
                .Include(c => c.ChildCategories)
                .Include(c => c.AuthorizedRoles)
                .Include(c => c.Category_Price)
                .Include(c => c.Category_Role_MaxDiscount)
                .Include(c => c.Category_InStock)
                .Include(c => c.Category_Price.Select(cp => cp.Contract))
                .SingleOrDefault(c => c.Id == id);
        }

        public Category GetCategoryByIdTracking(TCorpDbEntities ctx, int? id) {
            return ctx.Categories.Include(c => c.ChildCategories).Include(c => c.AuthorizedRoles).SingleOrDefault(c => c.Id == id);
        }
    }
}