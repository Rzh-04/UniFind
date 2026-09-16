using UniversityLostAndFound.Models;

namespace UniversityLostAndFound.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalItemsCount { get; set; }
        public int ActiveLostCount { get; set; }
        public int ActiveFoundCount { get; set; }
        public int ResolvedCount { get; set; }
        public int PendingClaimsCount { get; set; }

        public IEnumerable<Item> UserReportedItems { get; set; } = new List<Item>();
        public IEnumerable<Claim> UserClaims { get; set; } = new List<Claim>();
        public IEnumerable<Claim> PendingAdminClaims { get; set; } = new List<Claim>();
        public IEnumerable<Claim> ReviewedAdminClaims { get; set; } = new List<Claim>();
    }
}
