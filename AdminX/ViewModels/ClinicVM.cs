using Microsoft.EntityFrameworkCore;
using ClinicalXPDataConnections.Models;

namespace AdminX.ViewModels
{
    [Keyless]
    public class ClinicVM
    {
        public List<StaffMember> staffMembers { get; set; }
        public List<ActivityItem> activityItems { get; set; }
        public ActivityItem activityItem { get; set; }
        public Appointment Clinic { get; set; }
        public Referral linkedReferral { get; set; }
        public List<Outcome> outcomes { get; set; }        
        public Patient patient { get; set; }        
        public List<Appointment> outstandingClinicsList { get; set; }
        //public DateTime clinicFilterDate { get; set; }
        public string filterClinician { get; set; }
        public List<Referral> referralsList { get; set; }
        public List<ActivityType> appTypeList { get; set; }
        public List<ClinicVenue> venueList { get; set; }
        public string message { get; set; }
        public bool success { get; set; }
    }
}
