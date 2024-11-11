using AdminX.Models;
using Microsoft.EntityFrameworkCore;

namespace AdminX.ViewModels
{
    [Keyless]
    public class ReferralVM
    {        
        public Patient patient { get; set; }
        public List<ActivityType> activities { get; set; }
        public List<ExternalCliniciansAndFacilities> referrers { get; set; }
        public List<Priority> priority { get; set; }
        public List<StaffMember> consultants { get; set; }
        public List<StaffMember> gcs { get; set; }
        public List<StaffMember> admin { get; set; }
        public List<ActivityItem> referrals { get; set; }
        public List<string> pathways { get; set; }
    }
}
