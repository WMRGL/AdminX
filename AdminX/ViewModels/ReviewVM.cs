using Microsoft.EntityFrameworkCore;
using ClinicalXPDataConnections.Models;

namespace AdminX.ViewModels
{
    [Keyless]
    public class ReviewVM
    {
        public Patient patient { get; set; }
        public ActivityItem activityDetail { get; set; }
        public List<StaffMember> staffMembers { get; set; }
        public StaffMember staffMember { get; set; }
        public Review review { get; set; }
        public List<Review> reviewList { get; set; }
        public Referral referral { get; set; }
        public ActivityItem activity { get; set; }
        public List<ActivityItem> activityList { get; set; }
        public string message { get; set; }
        public bool success { get; set; }

    }
}
