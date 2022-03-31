using System;
using System.Data.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TCorp.Components;
using TCorp.EntityFramework;
using TCorp.Controllers;

namespace TCorp.Areas.ControlPanel.Controllers {
    public class KategorijeController : SuperAdminController {

        private AuthComponent login = new AuthComponent();
        private CategoryComponent categoryComponent = new CategoryComponent();

        public ActionResult Index(int? id = null) {
            using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                List<Category> childCategories = null;
                if (id == null) {
                    childCategories = categoryComponent.GetRootCategories(ctx);
                }
                else {
                    Category category = categoryComponent.GetCategoryById(ctx, id);
                    if (category == null) {
                        return new HttpNotFoundResult("Invalid id");
                    }
                    else {
                        childCategories = category.ChildCategories.ToList();
                    }
                }
                ViewBag.Mrvice = categoryComponent.GetAncestorsLinks(ctx, id);
                ViewBag.CreateId = id;
                return View(childCategories);
            }
        }

        public ActionResult Create(int? parentId = null) {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Create(string Name, int? parentId, bool IsHidden, string Description, string Info) {
            using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                Category c = new Category();
                c.Load(Name, parentId, IsHidden, Description, Info);
                ctx.Categories.Add(c);
                ctx.SaveChanges();
                return RedirectToAction("Index", new { id = parentId });
            }
        }

        [ValidateInput(false)]
        public ActionResult EditPredefinedDiscout(int id = 0) {
            if (id == 0) {
                return RedirectToAction("Index");
            }
            else {
                using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                    Category category = ctx.Categories.AsNoTracking().Include(c => c.PredefinedDiscount_Category).SingleOrDefault(cat => cat.Id == id);
                    if (category == null) {
                        return new HttpNotFoundResult("Invalid id");
                    }
                    else {
                        List<PredefinedDiscount> allDiscounts = ctx.PredefinedDiscount.AsNoTracking().ToList();
                        ViewBag.AllDiscounts = allDiscounts;
                        return View(category);
                    }
                }
            }
        }
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult EditPredefinedDiscout(int Id, List<decimal?> MonthlyDiscounts, List<decimal?> OneTimeDiscounts, List<int> PredefinedDiscounts) {
            if (Id == 0) {
                return RedirectToAction("Index");
            }
            else {
                using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                    Category category = ctx.Categories.SingleOrDefault(cat => cat.Id == Id);
                    if (category == null) {
                        return new HttpNotFoundResult("Invalid id");
                    }
                    else {
                        ctx.Database.ExecuteSqlCommand("DELETE FROM PredefinedDiscount_Category WHERE category_id = '" + Id + "'");
                        for (int i = 0; i < PredefinedDiscounts.Count; i++) {
                            decimal? monthlyDiscount = MonthlyDiscounts[i];
                            decimal? oneTimeDiscount = OneTimeDiscounts[i];
                            if (monthlyDiscount != null || oneTimeDiscount != null) {
                                PredefinedDiscount_Category pdc = new PredefinedDiscount_Category();
                                pdc.category_id = Id;
                                pdc.MonthlyPrice = monthlyDiscount;
                                pdc.OneTimePrice = oneTimeDiscount;
                                pdc.predefinedDiscount_id = PredefinedDiscounts[i];
                                ctx.PredefinedDiscount_Category.Add(pdc);
                            }
                        }
                        ctx.SaveChanges();
                        return RedirectToAction("Index", new { id = category.ParentId });
                    }
                }
            }
        }

        [ValidateInput(false)]
        public ActionResult Edit(int id = 0) {
            if (id == 0) {
                return RedirectToAction("Index");
            }
            else {
                using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                    Category category = categoryComponent.GetCategoryById(ctx, id);
                    if (category == null) {
                        return new HttpNotFoundResult("Invalid id");
                    }
                    else {
                        ViewBag.Contracts = ctx.Contract.AsNoTracking().ToList();
                        return View(category);
                    }
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(int Id, List<int> Contracts, List<string> MonthlyPrices, List<string> OneTimePrices, string Name, bool IsHidden, bool IsDeleted, string Description, bool ShowContractOfferName, string Info, string InitialQuantity, string InitialDiscount, string OfferName, string OfferNameOneTime, bool DoNotTrackStock, int? StockQuantity, bool UnknownQuantity) {
            using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                Category editedCategory = categoryComponent.GetCategoryByIdTracking(ctx, Id);
                if (editedCategory == null) {
                    return new HttpNotFoundResult("Invalid id");
                }
                int? quantity;
                decimal? discount;
                if (InitialDiscount == String.Empty) {
                    discount = null;
                }
                else {
                    discount = decimal.Parse(InitialDiscount);
                }
                if (InitialQuantity == String.Empty) {
                    quantity = null;
                }
                else {
                    quantity = int.Parse(InitialQuantity);
                }
                editedCategory.Update(Name, Contracts, MonthlyPrices, OneTimePrices, ShowContractOfferName, IsHidden, IsDeleted, Description, Info, quantity, discount, OfferName, OfferNameOneTime);
                Category_InStock stock = editedCategory.Category_InStock;
                if (DoNotTrackStock) {
                    if (stock != null) {
                        ctx.Category_InStock.Remove(stock);
                    }
                    editedCategory.Category_InStock = null;
                }
                else {
                    if (stock == null) {
                        stock = new Category_InStock();
                    }
                    stock.category_id = editedCategory.Id;
                    if (UnknownQuantity) {
                        stock.Quantity = null;
                    }
                    else {
                        stock.Quantity = StockQuantity;
                    }
                    editedCategory.Category_InStock = stock;
                }
                ctx.SaveChanges();
                return RedirectToAction("Index", new { id = editedCategory.ParentId });
            }
        }

        public ActionResult Delete(int id = 0) {
            if (id == 0) {
                return RedirectToAction("Index");
            }
            else {
                using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                    Category category = categoryComponent.GetCategoryByIdTracking(ctx, id);
                    if (category == null) {
                        return new HttpNotFoundResult("Invalid id");
                    }
                    else {
                        int? parentId = category.ParentId;
                        category.IsDeleted = true;
                        ctx.SaveChanges();
                        return RedirectToAction("Index", new { id = parentId });
                    }
                }
            }
        }
    }
}
