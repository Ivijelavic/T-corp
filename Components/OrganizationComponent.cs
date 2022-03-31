using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TCorp.EntityFramework;

namespace TCorp.Components {
    /// <summary>
    /// Handles all thing organization related
    /// </summary>
    public class OrganizationComponent {
        private const string ROOT_LINK = "<a href = \"./\">Root</a>";

        public LinkedList<string> GetAncestorsLinks(int? id) {
            LinkedList<string> ancestors = new LinkedList<string>();
            using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                while (id != null) {
                    var baseOrganization = ctx.Organization.AsNoTracking().SingleOrDefault(o => (id == null) ? o.Id.Equals(null) : o.Id == id);
                    if (baseOrganization == null) {
                        throw new ArgumentException("Invalid id");
                    }
                    string link = "<a href = \"" + baseOrganization.Id + "\">" + baseOrganization.Name + "</a>";
                    ancestors.AddFirst(link);
                    id = baseOrganization.parent_id;
                }
                ancestors.AddFirst(ROOT_LINK);
            }
            return ancestors;
        }

        /// <summary>
        /// Gets the depth of the organization in the tree
        /// </summary>
        /// <returns>Integer</returns>
        public int GetLevel(Organization organization) {
            int level = 1;
            using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                Organization currentOrg = organization;
                while (currentOrg.parent_id != null) {
                    currentOrg = ctx.Organization.Single(org => org.Id == currentOrg.parent_id);
                    level += 1;
                }
            }
            return level;
        }

        /// <summary>
        /// Prepares a SelectList of organizations to display. Only depth level 6 can be in the list
        /// </summary>
        /// <returns>A SelectList of organizations with a depth of 6</returns>
        public SelectList GetSelectList() {
            List<Organization> allOrgs;
            List<Organization> resultList = new List<Organization>();
            using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                allOrgs = ctx.Organization.AsNoTracking().OrderBy(o => o.Name).ToList();
            }
            foreach (Organization org in allOrgs) {
                if (GetLevel(org) == 6) {
                    resultList.Add(org);
                }
            }
            SelectList result = new SelectList(resultList, "Id", "Name");
            return result;
        }
        /// <summary>
        /// Prepares a SelectList of organizations to display, with one organization being preselected. Only depth level 6 can be in the list
        /// </summary>
        /// <returns>A SelectList of organizations with a depth of 6 with one organization preselected.</returns>
        public SelectList GetSelectList(int selected) {
            List<Organization> allOrgs;
            List<Organization> resultList = new List<Organization>();
            using (TCorpDbEntities ctx = new TCorpDbEntities()) {
                allOrgs = ctx.Organization.AsNoTracking().OrderBy(o => o.Name).ToList();
            }
            foreach (Organization org in allOrgs) {
                if (GetLevel(org) == 6) {
                    resultList.Add(org);
                }
            }
            SelectList result = new SelectList(resultList, "Id", "Name", selected);
            return result;
        }
    }
}