using ClinicalXPDataConnections.Models;
using Microsoft.EntityFrameworkCore;
using AdminX.Models;

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
        public Referral referral { get; set; }
        public List<ActivityItem> referrals { get; set; }
        public List<string> pathways { get; set; }
        public List<ListStatusAdmin> admin_status { get; set; }
        public List<ListDisease> diseases { get; set; }
        public Appointment Clinic { get; set; }
        public List<Appointment> ClinicList { get; set; }
        public List<ExternalFacility> facilities { get; set; }
        public List<Review> reviews { get; set; }
        public int clockAgeDays { get; set; }
        public int clockAgeWeeks { get; set; }
        public StaffMember staffMember { get; set; }
        public ActivityItem activity { get; set; }
        public Review review { get; set; }
    }
}
